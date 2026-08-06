using Application.Common.Models;
using Application.DTOs.Auth;
using Application.DTOs.Admin;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<object>> RegisterAsync(RegisterRequest request, string? ipAddress = null);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null);
    Task<ApiResponse<object>> LogoutAsync(Guid userId, string? ipAddress = null);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
    Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<ApiResponse<object>> VerifyEmailAsync(VerifyEmailRequest request);
    Task<ApiResponse<AuthResponse>> AdminLoginAsync(AdminLoginRequest request, string? ipAddress = null, string? userAgent = null);
}
