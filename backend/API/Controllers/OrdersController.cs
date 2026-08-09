using System.Security.Claims;
using Application.Common.Models;
using Application.DTOs.Storefront;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CustomerOrderResponse>>>> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _orderService.GetUserOrdersAsync(GetCurrentUserId(), page, pageSize);
        return Ok(ApiResponse<List<CustomerOrderResponse>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerOrderResponse>>> GetOrder(Guid id)
    {
        var result = await _orderService.GetOrderByIdAsync(GetCurrentUserId(), id);
        if (result == null)
            return NotFound(ApiResponse<CustomerOrderResponse>.ErrorResponse("Order not found", 404));
        return Ok(ApiResponse<CustomerOrderResponse>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<object>>> CancelOrder(Guid id)
    {
        var result = await _orderService.CancelOrderAsync(GetCurrentUserId(), id);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cannot cancel this order"));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Order cancelled"));
    }

    [HttpPut("{id:guid}/refund")]
    public async Task<ActionResult<ApiResponse<object>>> RequestRefund(Guid id, [FromBody] RefundRequest? request)
    {
        var result = await _orderService.RequestRefundAsync(GetCurrentUserId(), id, request?.Reason);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cannot request refund for this order"));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Refund request submitted"));
    }

    [HttpGet("{id:guid}/tracking")]
    public async Task<ActionResult<ApiResponse<OrderTrackingResponse>>> GetOrderTracking(Guid id)
    {
        var result = await _orderService.GetOrderTrackingAsync(GetCurrentUserId(), id);
        if (result == null)
            return NotFound(ApiResponse<OrderTrackingResponse>.ErrorResponse("Order not found", 404));
        return Ok(ApiResponse<OrderTrackingResponse>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}/invoice")]
    public async Task<ActionResult<ApiResponse<InvoiceResponse>>> GetOrderInvoice(Guid id)
    {
        var result = await _orderService.GetOrderInvoiceAsync(GetCurrentUserId(), id);
        if (result == null)
            return NotFound(ApiResponse<InvoiceResponse>.ErrorResponse("Order not found", 404));
        return Ok(ApiResponse<InvoiceResponse>.SuccessResponse(result));
    }
}

public class RefundRequest
{
    public string? Reason { get; set; }
}
