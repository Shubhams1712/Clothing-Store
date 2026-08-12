using Application.Common.Models;
using Application.DTOs.Admin;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Infrastructure.Services;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AuthService> _logger;
    private readonly EmailSettings _emailSettings;

    public AuthService(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IAuditService auditService,
        IEmailService emailService,
        IWebHostEnvironment env,
        ILogger<AuthService> logger,
        IOptions<EmailSettings> emailSettings)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _auditService = auditService;
        _emailService = emailService;
        _env = env;
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }

    public async Task<ApiResponse<object>> RegisterAsync(RegisterRequest request, string? ipAddress = null)
    {
        var existingUser = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (existingUser)
        {
            return ApiResponse<object>.ErrorResponse("An account with this email already exists", 409);
        }

        var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
        if (customerRole == null)
        {
            customerRole = new Role { Id = Guid.NewGuid(), Name = "Customer", Description = "Customer role", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true };
            _context.Roles.Add(customerRole);
            await _context.SaveChangesAsync();
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            IsEmailVerified = false,
            IsAdmin = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);

        var userRole = new UserRoleEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = customerRole.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.UserRoles.Add(userRole);

        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.EmailVerificationTokens.Add(verificationToken);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(user.Id, AuditAction.Registration, "User registered", ipAddress);

        if (_env.EnvironmentName == "Development")
        {
            var verificationLink = $"{_emailSettings.FrontendUrl.TrimEnd('/')}/verify-email?token={Uri.EscapeDataString(verificationToken.Token)}&email={Uri.EscapeDataString(user.Email)}";
            _logger.LogInformation("=== EMAIL VERIFICATION (Development Mode) ===");
            _logger.LogInformation("User: {Email}", user.Email);
            _logger.LogInformation("Verification Link: {Link}", verificationLink);
            _logger.LogInformation("============================================");

            user.IsEmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        else
        {
            var emailSent = await _emailService.SendVerificationEmailAsync(
                user.Email,
                verificationToken.Token,
                user.Email);

            if (!emailSent)
            {
                _logger.LogWarning("Failed to send verification email to {Email}. User can request a new verification email.", user.Email);
            }
        }

        return ApiResponse<object>.SuccessResponse(new { }, "Registration successful. Please verify your email.");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            await _auditService.LogAsync(null, AuditAction.FailedLogin, $"Failed login attempt for {request.Email}", ipAddress, userAgent, false);
            return ApiResponse<AuthResponse>.ErrorResponse("Invalid email or password", 401);
        }

        if (!user.IsActive)
        {
            return ApiResponse<AuthResponse>.ErrorResponse("Account has been deactivated", 403);
        }

        if (!user.IsEmailVerified)
        {
            return ApiResponse<AuthResponse>.ErrorResponse("Please verify your email before logging in", 403);
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        if (user.IsAdmin && !roles.Contains("Admin"))
            roles.Add("Admin");

        _logger.LogWarning("[AUTH LOGIN] User: {UserId} ({Email}), IsAdmin: {IsAdmin}, UserRoles: {UserRoles}, FinalRoles: {FinalRoles}",
            user.Id, user.Email, user.IsAdmin, string.Join(",", user.UserRoles.Select(ur => ur.Role.Name)), string.Join(",", roles));

        var userResponse = MapToUserResponse(user, roles);
        var tokens = _tokenService.GenerateTokens(userResponse);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokens.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(user.Id, AuditAction.Login, "User logged in", ipAddress, userAgent);

        return ApiResponse<AuthResponse>.SuccessResponse(tokens, "Login successful");
    }

    public async Task<ApiResponse<object>> LogoutAsync(Guid userId, string? ipAddress = null)
    {
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
            token.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(userId, AuditAction.Logout, "User logged out", ipAddress);

        return ApiResponse<object>.SuccessResponse(new { }, "Logout successful");
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return ApiResponse<AuthResponse>.ErrorResponse("Invalid or expired refresh token", 401);
        }

        var user = storedToken.User;

        if (!user.IsActive)
        {
            return ApiResponse<AuthResponse>.ErrorResponse("Account has been deactivated", 403);
        }

        storedToken.IsRevoked = true;
        storedToken.ReplacedByToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        storedToken.UpdatedAt = DateTime.UtcNow;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        if (user.IsAdmin && !roles.Contains("Admin"))
            roles.Add("Admin");
        var userResponse = MapToUserResponse(user, roles);
        var newTokens = _tokenService.GenerateTokens(userResponse);

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newTokens.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return ApiResponse<AuthResponse>.SuccessResponse(newTokens, "Token refreshed");
    }

    public async Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return ApiResponse<object>.SuccessResponse(new { }, "If the email exists, a reset link has been sent");
        }

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(user.Id, AuditAction.PasswordReset, "Password reset requested");

        if (_env.EnvironmentName != "Development")
        {
            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                resetToken.Token,
                user.Email);
        }
        else
        {
            var resetLink = $"{_emailSettings.FrontendUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(resetToken.Token)}&email={Uri.EscapeDataString(user.Email)}";
            _logger.LogInformation("=== PASSWORD RESET (Development Mode) ===");
            _logger.LogInformation("User: {Email}", user.Email);
            _logger.LogInformation("Reset Link: {Link}", resetLink);
            _logger.LogInformation("==========================================");
        }

        return ApiResponse<object>.SuccessResponse(new { }, "If the email exists, a reset link has been sent");
    }

    public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return ApiResponse<object>.ErrorResponse("Invalid reset token", 400);
        }

        var resetToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id && t.Token == request.Token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

        if (resetToken == null)
        {
            return ApiResponse<object>.ErrorResponse("Invalid or expired reset token", 400);
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        resetToken.IsUsed = true;
        resetToken.UpdatedAt = DateTime.UtcNow;

        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
            token.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(user.Id, AuditAction.PasswordReset, "Password reset completed");

        return ApiResponse<object>.SuccessResponse(new { }, "Password reset successful");
    }

    public async Task<ApiResponse<object>> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return ApiResponse<object>.ErrorResponse("Invalid verification token", 400);
        }

        var verificationToken = await _context.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id && t.Token == request.Token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

        if (verificationToken == null)
        {
            return ApiResponse<object>.ErrorResponse("Invalid or expired verification token", 400);
        }

        user.IsEmailVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        verificationToken.IsUsed = true;
        verificationToken.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(user.Id, AuditAction.EmailVerification, "Email verified");

        return ApiResponse<object>.SuccessResponse(new { }, "Email verified successfully");
    }

    public async Task<ApiResponse<AuthResponse>> AdminLoginAsync(AdminLoginRequest request, string? ipAddress = null, string? userAgent = null)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            await _auditService.LogAsync(null, AuditAction.FailedLogin, $"Failed admin login attempt for {request.Email}", ipAddress, userAgent, false);
            return ApiResponse<AuthResponse>.ErrorResponse("Invalid email or password", 401);
        }

        if (!user.IsAdmin)
        {
            await _auditService.LogAsync(user.Id, AuditAction.FailedLogin, "Non-admin user attempted admin login", ipAddress, userAgent, false);
            return ApiResponse<AuthResponse>.ErrorResponse("Access denied", 403);
        }

        if (!user.IsActive)
        {
            return ApiResponse<AuthResponse>.ErrorResponse("Account has been deactivated", 403);
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        if (user.IsAdmin && !roles.Contains("Admin"))
            roles.Add("Admin");
        var userResponse = MapToUserResponse(user, roles);
        var tokens = _tokenService.GenerateTokens(userResponse);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokens.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(user.Id, AuditAction.Login, "Admin logged in", ipAddress, userAgent);

        return ApiResponse<AuthResponse>.SuccessResponse(tokens, "Admin login successful");
    }

    private static UserResponse MapToUserResponse(User user, IEnumerable<string> roles)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsEmailVerified = user.IsEmailVerified,
            IsAdmin = user.IsAdmin,
            Roles = roles.ToList()
        };
    }
}
