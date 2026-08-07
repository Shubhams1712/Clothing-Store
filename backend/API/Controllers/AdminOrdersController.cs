using Application.Common.Models;
using Application.DTOs.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin,Manager,Staff")]
public class AdminOrdersController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminOrdersController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<OrderResponse>>>> GetOrders([FromQuery] PaginatedRequest request)
    {
        var result = await _adminService.GetOrdersAsync(request);
        return Ok(ApiResponse<PaginatedResponse<OrderResponse>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> GetOrder(Guid id)
    {
        var result = await _adminService.GetOrderByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<OrderResponse>.ErrorResponse("Order not found", 404));
        return Ok(ApiResponse<OrderResponse>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await _adminService.UpdateOrderStatusAsync(id, request);
        if (result == null) return NotFound(ApiResponse<OrderResponse>.ErrorResponse("Order not found", 404));
        return Ok(ApiResponse<OrderResponse>.SuccessResponse(result, "Order status updated"));
    }
}
