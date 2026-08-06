using Application.DTOs.Auth;

namespace Application.Interfaces;

public interface ITokenService
{
    AuthResponse GenerateTokens(DTOs.Auth.UserResponse user);
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}
