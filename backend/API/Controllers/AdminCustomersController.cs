using System.Security.Claims;
using Application.Common.Models;
using Application.DTOs.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/admin/customers")]
[Authorize(Roles = "Admin,Manager")]
[EnableRateLimiting("global")]
public class AdminCustomersController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminCustomersController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CustomerResponse>>>> GetCustomers([FromQuery] PaginatedRequest request)
    {
        var result = await _adminService.GetCustomersAsync(request);
        return Ok(ApiResponse<PaginatedResponse<CustomerResponse>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerDetailResponse>>> GetCustomer(Guid id)
    {
        var result = await _adminService.GetCustomerByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<CustomerDetailResponse>.ErrorResponse("Customer not found", 404));
        return Ok(ApiResponse<CustomerDetailResponse>.SuccessResponse(result));
    }

    [HttpPatch("{id:guid}/toggle-active")]
    public async Task<ActionResult<ApiResponse<object>>> ToggleActive(Guid id)
    {
        var result = await _adminService.ToggleCustomerActiveAsync(id);
        if (!result) return NotFound(ApiResponse<object>.ErrorResponse("Customer not found", 404));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Customer status toggled"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/promote")]
    public async Task<ActionResult<ApiResponse<object>>> PromoteToAdmin(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == id)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cannot promote yourself", 400));

        var result = await _adminService.SetUserAdminStatusAsync(id, isAdmin: true);
        if (!result) return NotFound(ApiResponse<object>.ErrorResponse("User not found", 404));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "User promoted to admin"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/demote")]
    public async Task<ActionResult<ApiResponse<object>>> DemoteFromAdmin(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == id)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cannot demote yourself", 400));

        var result = await _adminService.SetUserAdminStatusAsync(id, isAdmin: false);
        if (!result) return NotFound(ApiResponse<object>.ErrorResponse("User not found", 404));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Admin privileges removed"));
    }
}
