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
}
