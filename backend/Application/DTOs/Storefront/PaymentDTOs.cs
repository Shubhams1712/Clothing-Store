using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Storefront;

public class CreatePaymentOrderRequest
{
    [Required]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "INR";

    public string? Receipt { get; set; }
}

public class PaymentOrderResponse
{
    public string OrderId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string KeyId { get; set; } = string.Empty;
}

public class VerifyPaymentRequest
{
    [Required]
    public string RazorpayOrderId { get; set; } = string.Empty;

    [Required]
    public string RazorpayPaymentId { get; set; } = string.Empty;

    [Required]
    public string RazorpaySignature { get; set; } = string.Empty;

    public string? CouponCode { get; set; }
}

public class CreateOrderFromPaymentRequest
{
    [Required]
    public string RazorpayOrderId { get; set; } = string.Empty;

    [Required]
    public string RazorpayPaymentId { get; set; } = string.Empty;

    [Required]
    public string RazorpaySignature { get; set; } = string.Empty;

    [Required]
    public List<CheckoutItemRequest> Items { get; set; } = new();

    public string? CouponCode { get; set; }

    public Guid? ShippingAddressId { get; set; }

    public string? ShippingMethod { get; set; }

    public string? Notes { get; set; }
}

public class CreateCodOrderRequest
{
    [Required]
    public List<CheckoutItemRequest> Items { get; set; } = new();

    public string? CouponCode { get; set; }

    public Guid? ShippingAddressId { get; set; }

    public string? ShippingMethod { get; set; }

    public string? Notes { get; set; }
}

public class CustomerOrderResponse
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Currency { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaymentId { get; set; }
    public string? ShippingName { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; }
    public string? ShippingPhone { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public List<CustomerOrderItemResponse> Items { get; set; } = new();
}

public class CustomerOrderItemResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
}

public class OrderTrackingResponse
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? TrackingNumber { get; set; }
    public List<OrderTrackingEvent> Timeline { get; set; } = new();
}

public class OrderTrackingEvent
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Description { get; set; }
}

public class InvoiceResponse
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? BillingAddress { get; set; }
    public string ShippingName { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; }
    public List<InvoiceItemResponse> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentId { get; set; }
}

public class InvoiceItemResponse
{
    public string ProductName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
