using Application.Common.Models;
using Application.DTOs.Admin;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin,Manager,Staff")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminDashboardController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardStatsResponse>>> GetDashboardStats()
    {
        var result = await _adminService.GetDashboardStatsAsync();
        return Ok(ApiResponse<DashboardStatsResponse>.SuccessResponse(result));
    }
}
