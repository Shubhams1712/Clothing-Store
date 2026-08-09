using Application.DTOs.Storefront;

namespace Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentOrderResponse> CreateRazorpayOrderAsync(Guid userId, decimal amount, string currency, string? receipt);
    Task<bool> VerifyPaymentAsync(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature);
    Task<string> GetRazorpayKeyIdAsync();
}
