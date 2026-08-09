using Application.DTOs.Storefront;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<CustomerOrderResponse?> GetOrderByIdAsync(Guid userId, Guid orderId);
    Task<List<CustomerOrderResponse>> GetUserOrdersAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<CustomerOrderResponse> CreateOrderAsync(Guid userId, CreateOrderFromPaymentRequest request);
    Task<CustomerOrderResponse?> CreateCodOrderAsync(Guid userId, CreateCodOrderRequest request);
    Task<bool> CancelOrderAsync(Guid userId, Guid orderId);
    Task<bool> RequestRefundAsync(Guid userId, Guid orderId, string? reason);
    Task<OrderTrackingResponse?> GetOrderTrackingAsync(Guid userId, Guid orderId);
    Task<InvoiceResponse?> GetOrderInvoiceAsync(Guid userId, Guid orderId);
}
