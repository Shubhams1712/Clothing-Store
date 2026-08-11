using System.Security.Claims;
using Application.Common.Models;
using Application.DTOs.Storefront;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
[EnableRateLimiting("global")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;

    public PaymentsController(IPaymentService paymentService, IOrderService orderService)
    {
        _paymentService = paymentService;
        _orderService = orderService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    [HttpPost("create-order")]
    public async Task<ActionResult<ApiResponse<PaymentOrderResponse>>> CreatePaymentOrder([FromBody] CreatePaymentOrderRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _paymentService.CreateRazorpayOrderAsync(userId, request.Amount, request.Currency, request.Receipt);
        return Ok(ApiResponse<PaymentOrderResponse>.SuccessResponse(result));
    }

    [HttpPost("create-order-after-payment")]
    public async Task<ActionResult<ApiResponse<CustomerOrderResponse>>> CreateOrderFromPayment([FromBody] CreateOrderFromPaymentRequest request)
    {
        var userId = GetCurrentUserId();
        try
        {
            var result = await _orderService.CreateOrderAsync(userId, request);
            return Ok(ApiResponse<CustomerOrderResponse>.SuccessResponse(result, "Order created", 201));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CustomerOrderResponse>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("cod")]
    public async Task<ActionResult<ApiResponse<CustomerOrderResponse>>> CreateCodOrder([FromBody] CreateCodOrderRequest request)
    {
        var userId = GetCurrentUserId();
        try
        {
            var result = await _orderService.CreateCodOrderAsync(userId, request);
            if (result == null)
                return BadRequest(ApiResponse<CustomerOrderResponse>.ErrorResponse("Failed to create COD order"));
            return Ok(ApiResponse<CustomerOrderResponse>.SuccessResponse(result, "COD order created", 201));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CustomerOrderResponse>.ErrorResponse(ex.Message));
        }
    }
}
