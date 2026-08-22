using System.Security.Cryptography;
using System.Text;
using Application.DTOs.Storefront;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Razorpay.Api;

namespace Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ApplicationDbContext context, IConfiguration configuration, ILogger<PaymentService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PaymentOrderResponse> CreateRazorpayOrderAsync(Guid userId, decimal amount, string currency, string? receipt)
    {
        var (keyId, keySecret) = await GetRazorpayCredentialsAsync();

        if (string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(keySecret))
            throw new InvalidOperationException("Razorpay credentials not configured. Please set them in Admin > Settings or via environment variables.");

        if (amount <= 0)
            throw new InvalidOperationException($"Invalid payment amount: {amount}. Amount must be greater than zero.");

        amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

        try
        {
            var client = new RazorpayClient(keyId, keySecret);

            var orderRequest = new Dictionary<string, object>
            {
                { "amount", (long)(amount * 100) },
                { "currency", currency },
                { "receipt", receipt ?? Guid.NewGuid().ToString() }
            };

            _logger.LogInformation("Creating Razorpay order: Amount={Amount} ({AmountPaise} paise), Currency={Currency}, Mode={Mode}",
                amount, (long)(amount * 100), currency, keyId.StartsWith("rzp_test") ? "TEST" : "LIVE");

            var order = client.Order.Create(orderRequest);

            return new PaymentOrderResponse
            {
                OrderId = order["id"].ToString()!,
                Amount = (long)(amount * 100),
                Currency = currency,
                KeyId = keyId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Razorpay order for amount {Amount}", amount);
            throw new InvalidOperationException($"Failed to create payment order: {ex.Message}");
        }
    }

    public async Task<bool> VerifyPaymentAsync(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
    {
        var (_, keySecret) = await GetRazorpayCredentialsAsync();

        if (string.IsNullOrEmpty(keySecret))
            throw new InvalidOperationException("Razorpay key secret not configured");

        try
        {
            var attributes = new Dictionary<string, string>
            {
                { "razorpay_order_id", razorpayOrderId },
                { "razorpay_payment_id", razorpayPaymentId },
                { "razorpay_signature", razorpaySignature }
            };

            Utils.verifyPaymentSignature(attributes);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Payment signature verification failed for order {OrderId}", razorpayOrderId);
            return false;
        }
    }

    public async Task<string> GetRazorpayKeyIdAsync()
    {
        var (keyId, _) = await GetRazorpayCredentialsAsync();
        return keyId ?? string.Empty;
    }

    public async Task<long?> GetRazorpayOrderAmountAsync(string razorpayOrderId)
    {
        try
        {
            var (keyId, keySecret) = await GetRazorpayCredentialsAsync();
            if (string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(keySecret))
                return null;

            var client = new RazorpayClient(keyId, keySecret);
            var order = client.Order.Fetch(razorpayOrderId);
            if (order != null && order.Attributes.ContainsKey("amount"))
            {
                return Convert.ToInt64(order["amount"]);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Razorpay order amount for {OrderId}", razorpayOrderId);
            return null;
        }
    }

    public bool VerifyWebhookSignature(string payload, string signature)
    {
        var webhookSecret = GetRazorpayWebhookSecretAsync().GetAwaiter().GetResult();

        if (string.IsNullOrEmpty(webhookSecret))
            return false;

        try
        {
            var keyBytes = Encoding.UTF8.GetBytes(webhookSecret);
            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedSignature),
                Encoding.UTF8.GetBytes(signature));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Webhook signature verification failed");
            return false;
        }
    }

    private async Task<string?> GetRazorpayWebhookSecretAsync()
    {
        var settings = await _context.StoreSettings.FirstOrDefaultAsync();

        if (settings != null && !string.IsNullOrEmpty(settings.RazorpayWebhookSecret))
            return settings.RazorpayWebhookSecret;

        var configSecret = _configuration["Razorpay:WebhookSecret"];
        if (!string.IsNullOrEmpty(configSecret))
            return configSecret;

        return null;
    }

    private async Task<(string? KeyId, string? KeySecret)> GetRazorpayCredentialsAsync()
    {
        var settings = await _context.StoreSettings.FirstOrDefaultAsync();

        if (settings != null && !string.IsNullOrEmpty(settings.RazorpayKeyId) && !string.IsNullOrEmpty(settings.RazorpayKeySecret))
            return (settings.RazorpayKeyId, settings.RazorpayKeySecret);

        var configKeyId = _configuration["Razorpay:KeyId"];
        var configKeySecret = _configuration["Razorpay:KeySecret"];

        if (!string.IsNullOrEmpty(configKeyId) && !string.IsNullOrEmpty(configKeySecret))
            return (configKeyId, configKeySecret);

        return (null, null);
    }
}
