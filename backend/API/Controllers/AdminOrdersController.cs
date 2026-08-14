using Application.Common.Models;
using Application.DTOs.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin,Manager,Staff")]
[EnableRateLimiting("global")]
public class AdminOrdersController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IFulfillmentService _fulfillmentService;

    public AdminOrdersController(IAdminService adminService, IFulfillmentService fulfillmentService)
    {
        _adminService = adminService;
        _fulfillmentService = fulfillmentService;
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

    [HttpGet("{id:guid}/fulfillment")]
    public async Task<ActionResult<ApiResponse<FulfillmentOrderResponse>>> GetFulfillmentOrder(Guid id)
    {
        var result = await _fulfillmentService.GetFulfillmentOrderAsync(id);
        if (result == null) return NotFound(ApiResponse<FulfillmentOrderResponse>.ErrorResponse("Fulfillment order not found", 404));
        return Ok(ApiResponse<FulfillmentOrderResponse>.SuccessResponse(result));
    }
}
