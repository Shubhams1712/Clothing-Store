using Application.Common.Models;
using Application.DTOs.User;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<UserProfileResponse>> GetProfileAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return ApiResponse<UserProfileResponse>.ErrorResponse("User not found", 404);
        }

        var response = new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };

        return ApiResponse<UserProfileResponse>.SuccessResponse(response);
    }

    public async Task<ApiResponse<UserProfileResponse>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return ApiResponse<UserProfileResponse>.ErrorResponse("User not found", 404);
        }

        if (request.FirstName != null)
            user.FirstName = request.FirstName;
        if (request.LastName != null)
            user.LastName = request.LastName;

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var response = new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };

        return ApiResponse<UserProfileResponse>.SuccessResponse(response, "Profile updated");
    }

    public async Task<ApiResponse<object>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return ApiResponse<object>.ErrorResponse("User not found", 404);
        }

        if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
        {
            return ApiResponse<object>.ErrorResponse("Current password is incorrect", 400);
        }

        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
            token.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<object>.SuccessResponse(new { }, "Password changed successfully");
    }
}
