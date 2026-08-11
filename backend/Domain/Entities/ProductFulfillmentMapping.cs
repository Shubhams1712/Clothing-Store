using Domain.Common;

namespace Domain.Entities;

public class ProductFulfillmentMapping : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public Guid ProviderId { get; set; }
    public FulfillmentProvider Provider { get; set; } = null!;
    public string ExternalProductId { get; set; } = string.Empty;
    public string? ExternalVariantId { get; set; }
    public string ExternalSku { get; set; } = string.Empty;
    public string? DesignReference { get; set; }
    public string? DesignFileUrl { get; set; }
    public string? PrintingType { get; set; }
    public string? PrintingPlacement { get; set; }
}
