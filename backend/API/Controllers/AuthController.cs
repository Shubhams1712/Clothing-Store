using Application.Common.Models;
using Application.DTOs.Auth;
using Application.DTOs.Admin;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
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
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return Ok(result);
    }

    [HttpPost("reset-password")]
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
