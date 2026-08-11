using System.Security.Cryptography;
using System.Text;
using Application.DTOs.Storefront;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;

namespace Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public PaymentService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<PaymentOrderResponse> CreateRazorpayOrderAsync(Guid userId, decimal amount, string currency, string? receipt)
    {
        var (keyId, keySecret) = await GetRazorpayCredentialsAsync();

        if (string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(keySecret))
            throw new InvalidOperationException("Razorpay credentials not configured. Please set them in Admin > Settings or via environment variables.");

        var client = new RazorpayClient(keyId, keySecret);

        var orderRequest = new Dictionary<string, object>
        {
            { "amount", (long)(amount * 100) },
            { "currency", currency },
            { "receipt", receipt ?? Guid.NewGuid().ToString() }
        };

        var order = client.Order.Create(orderRequest);

        return new PaymentOrderResponse
        {
            OrderId = order["id"].ToString()!,
            Amount = amount,
            Currency = currency,
            KeyId = keyId
        };
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
        catch
        {
            return false;
        }
    }

    public async Task<string> GetRazorpayKeyIdAsync()
    {
        var (keyId, _) = await GetRazorpayCredentialsAsync();
        return keyId ?? string.Empty;
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
        catch
        {
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
