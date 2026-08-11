using Domain.Common;

namespace Domain.Entities;

public class FulfillmentOrderItem : BaseEntity
{
    public Guid FulfillmentOrderId { get; set; }
    public FulfillmentOrder FulfillmentOrder { get; set; } = null!;
    public Guid OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = null!;
    public string ExternalProductId { get; set; } = string.Empty;
    public string? ExternalVariantId { get; set; }
    public string ExternalSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Status { get; set; }
    public string? FailureReason { get; set; }
    public string? DesignReference { get; set; }
    public string? DesignFileUrl { get; set; }
    public string? MockupUrl { get; set; }
}
