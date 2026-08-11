using Application.Common.Models;
using Application.DTOs.Auth;
using Application.DTOs.User;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("global")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetProfile()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _userService.GetProfileAsync(userId.Value);
        return result.StatusCode switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => BadRequest(result)
        };
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _userService.UpdateProfileAsync(userId.Value, request);
        return result.StatusCode switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => BadRequest(result)
        };
    }

    [HttpPut("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _userService.ChangePasswordAsync(userId.Value, request.CurrentPassword, request.NewPassword);
        return result.StatusCode switch
        {
            200 => Ok(result),
            400 => BadRequest(result),
            404 => NotFound(result),
            _ => BadRequest(result)
        };
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }
}
