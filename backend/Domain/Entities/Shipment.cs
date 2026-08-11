using Domain.Common;

namespace Domain.Entities;

public class Shipment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid? FulfillmentOrderId { get; set; }
    public FulfillmentOrder? FulfillmentOrder { get; set; }
    public string? TrackingNumber { get; set; }
    public string? CourierName { get; set; }
    public string? TrackingUrl { get; set; }
    public string? ProviderShippingStatus { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
