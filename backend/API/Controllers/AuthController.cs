using System.Security.Claims;
using Application.Common.Models;
using Application.DTOs.Auth;
using Application.DTOs.Admin;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("global")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ApplicationDbContext context, ILogger<AuthController> logger)
    {
        _authService = authService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Diagnostic endpoint: shows current user's JWT claims and database state.
    /// Requires authentication. No secrets are logged.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ApiResponse<object>>> Me()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var roleClaims = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var isAdminClaim = User.FindFirst("is_admin")?.Value;
        var allClaimTypes = User.Claims.Select(c => c.Type.Split('/').Last()).ToList();

        var response = new Dictionary<string, object>
        {
            ["jwtClaims"] = new
            {
                userId,
                email,
                roleClaims,
                isAdminClaim,
                allClaimTypes
            },
            ["database"] = new Dictionary<string, object>()
        };

        if (Guid.TryParse(userId, out var parsedUserId))
        {
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == parsedUserId);
            var dbRoles = await _context.UserRoles
                .Where(ur => ur.UserId == parsedUserId && ur.IsActive)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync();

            if (dbUser != null)
            {
                response["database"] = new Dictionary<string, object>
                {
                    ["dbIsAdmin"] = dbUser.IsAdmin,
                    ["dbRoles"] = dbRoles,
                    ["dbIsActive"] = dbUser.IsActive,
                    ["dbIsEmailVerified"] = dbUser.IsEmailVerified
                };
            }
        }

        _logger.LogWarning("[AUTH ME] User: {UserId}, JWT Roles: {JwtRoles}, IsAdminClaim: {IsAdminClaim}",
            userId, string.Join(",", roleClaims), isAdminClaim);

        return Ok(ApiResponse<object>.SuccessResponse(response, "Diagnostic info"));
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request, GetIpAddress());
        return result.StatusCode switch
        {
            200 => Ok(result),
            409 => Conflict(result),
            _ => BadRequest(result)
        };
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request, GetIpAddress(), GetUserAgent());
        return result.StatusCode switch
        {
            200 => Ok(result),
            401 => Unauthorized(result),
            403 => StatusCode(403, result),
            _ => BadRequest(result)
        };
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _authService.LogoutAsync(userId.Value, GetIpAddress());
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, GetIpAddress());
        return result.StatusCode switch
        {
            200 => Ok(result),
            401 => Unauthorized(result),
            _ => BadRequest(result)
        };
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return result.StatusCode switch
        {
            200 => Ok(result),
            400 => BadRequest(result),
            _ => BadRequest(result)
        };
    }

    [HttpPost("verify-email")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var result = await _authService.VerifyEmailAsync(request);
        return result.StatusCode switch
        {
            200 => Ok(result),
            400 => BadRequest(result),
            _ => BadRequest(result)
        };
    }

    [HttpPost("admin/login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> AdminLogin([FromBody] AdminLoginRequest request)
    {
        var result = await _authService.AdminLoginAsync(request, GetIpAddress(), GetUserAgent());
        return result.StatusCode switch
        {
            200 => Ok(result),
            401 => Unauthorized(result),
            403 => StatusCode(403, result),
            _ => BadRequest(result)
        };
    }

    private string? GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        return HttpContext.Request.Headers.UserAgent.ToString();
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }
}
