using Application.Common.Models;
using Application.DTOs.Admin;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/storefront/settings")]
[EnableRateLimiting("global")]
public class StorefrontSettingsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public StorefrontSettingsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<StoreSettingsResponse>>> GetSettings()
    {
        var result = await _adminService.GetSettingsAsync();
        if (result == null)
        {
            return Ok(ApiResponse<StoreSettingsResponse>.SuccessResponse(new StoreSettingsResponse
            {
                Id = Guid.Empty,
                StoreName = "The Freak Store"
            }));
        }
        return Ok(ApiResponse<StoreSettingsResponse>.SuccessResponse(result));
    }
}
