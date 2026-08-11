using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class FulfillmentOrder : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid ProviderId { get; set; }
    public FulfillmentProvider Provider { get; set; } = null!;
    public string? ExternalOrderId { get; set; }
    public FulfillmentStatus Status { get; set; } = FulfillmentStatus.Pending;
    public string? ProviderStatus { get; set; }
    public string? FailureReason { get; set; }
    public string? ErrorCategory { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ICollection<FulfillmentOrderItem> Items { get; set; } = new List<FulfillmentOrderItem>();
    public Shipment? Shipment { get; set; }
}
