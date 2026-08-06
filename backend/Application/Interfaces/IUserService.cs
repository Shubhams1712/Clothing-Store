using Application.Common.Models;
using Application.DTOs.User;

namespace Application.Interfaces;

public interface IUserService
{
    Task<ApiResponse<UserProfileResponse>> GetProfileAsync(Guid userId);
    Task<ApiResponse<UserProfileResponse>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<ApiResponse<object>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
}
