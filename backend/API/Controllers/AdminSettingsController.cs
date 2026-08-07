using Application.Common.Models;
using Application.DTOs.Admin;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Roles = "Admin")]
public class AdminSettingsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminSettingsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<StoreSettingsResponse>>> GetSettings()
    {
        var result = await _adminService.GetSettingsAsync();
        if (result == null) return Ok(ApiResponse<StoreSettingsResponse>.SuccessResponse(new StoreSettingsResponse { StoreName = "My Store" }));
        return Ok(ApiResponse<StoreSettingsResponse>.SuccessResponse(result));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<StoreSettingsResponse>>> UpdateSettings([FromBody] UpdateStoreSettingsRequest request)
    {
        var result = await _adminService.UpdateSettingsAsync(request);
        return Ok(ApiResponse<StoreSettingsResponse>.SuccessResponse(result, "Settings updated"));
    }
}
