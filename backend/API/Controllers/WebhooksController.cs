using System.Text.Json;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

namespace API.Controllers;

[ApiController]
[Route("api/payments")]
public class WebhooksController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;

    public WebhooksController(IPaymentService paymentService, IOrderService orderService)
    {
        _paymentService = paymentService;
        _orderService = orderService;
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    [DisableRateLimiting]
    public async Task<IActionResult> RazorpayWebhook()
    {
        string payload;
        using (var reader = new StreamReader(Request.Body))
        {
            payload = await reader.ReadToEndAsync();
        }

        var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();

        if (string.IsNullOrEmpty(signature))
        {
            Log.Warning("Webhook received without signature");
            return BadRequest(new { error = "Missing signature" });
        }

        if (!_paymentService.VerifyWebhookSignature(payload, signature))
        {
            Log.Warning("Webhook signature verification failed");
            return BadRequest(new { error = "Invalid signature" });
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(payload);
        }
        catch (JsonException)
        {
            Log.Warning("Webhook received invalid JSON payload");
            return BadRequest(new { error = "Invalid payload" });
        }

        if (!doc.RootElement.TryGetProperty("event", out var eventElement))
        {
            return BadRequest(new { error = "Missing event type" });
        }

        var eventType = eventElement.GetString();

        if (!doc.RootElement.TryGetProperty("payload", out var payloadElement))
        {
            return BadRequest(new { error = "Missing payload" });
        }

        try
        {
            switch (eventType)
            {
                case "payment.captured":
                    await HandlePaymentCapturedAsync(payloadElement);
                    break;
                case "payment.failed":
                    await HandlePaymentFailedAsync(payloadElement);
                    break;
                default:
                    Log.Information("Unhandled webhook event: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing webhook event {EventType}", eventType);
            return StatusCode(500, new { error = "Webhook processing failed" });
        }

        return Ok(new { status = "ok" });
    }

    private async Task HandlePaymentCapturedAsync(JsonElement payloadElement)
    {
        var payment = ExtractPayment(payloadElement);
        if (payment == null) return;

        var (orderId, paymentId) = payment.Value;
        await _orderService.HandleWebhookPaymentCapturedAsync(orderId, paymentId);

        Log.Information("Webhook payment.captured processed for payment {PaymentId}", paymentId);
    }

    private async Task HandlePaymentFailedAsync(JsonElement payloadElement)
    {
        var payment = ExtractPayment(payloadElement);
        if (payment == null) return;

        var (orderId, paymentId) = payment.Value;
        await _orderService.HandleWebhookPaymentFailedAsync(orderId, paymentId);

        Log.Information("Webhook payment.failed processed for payment {PaymentId}", paymentId);
    }

    private static (string RazorpayOrderId, string RazorpayPaymentId)? ExtractPayment(JsonElement payloadElement)
    {
        if (!payloadElement.TryGetProperty("payment", out var paymentWrapper))
            return null;

        if (!paymentWrapper.TryGetProperty("entity", out var paymentEntity))
            return null;

        var razorpayPaymentId = paymentEntity.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";
        var razorpayOrderId = "";

        if (paymentEntity.TryGetProperty("order_id", out var orderIdProp))
            razorpayOrderId = orderIdProp.GetString() ?? "";

        if (string.IsNullOrEmpty(razorpayPaymentId))
            return null;

        return (razorpayOrderId, razorpayPaymentId);
    }
}
