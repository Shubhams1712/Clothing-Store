namespace Domain.Enums;

public enum OrderStatus
{
    PendingPayment = 0,
    PaymentFailed = 1,
    PaymentSuccessful = 2,
    Confirmed = 3,
    Packed = 4,
    Shipped = 5,
    OutForDelivery = 6,
    Delivered = 7,
    Cancelled = 8,
    RefundRequested = 9,
    Refunded = 10
}
