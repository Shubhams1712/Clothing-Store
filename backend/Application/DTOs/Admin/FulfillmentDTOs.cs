namespace Application.DTOs.Admin;

public class FulfillmentOrderResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public string? ExternalOrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ProviderStatus { get; set; }
    public string? FailureReason { get; set; }
    public string? ErrorCategory { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<FulfillmentOrderItemResponse> Items { get; set; } = new();
    public ShipmentResponse? Shipment { get; set; }
}

public class FulfillmentOrderItemResponse
{
    public Guid Id { get; set; }
    public Guid FulfillmentOrderId { get; set; }
    public Guid OrderItemId { get; set; }
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

public class ShipmentResponse
{
    public Guid Id { get; set; }
    public Guid FulfillmentOrderId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? CourierName { get; set; }
    public string? TrackingUrl { get; set; }
    public string? ProviderShippingStatus { get; set; }
}

public class FulfillmentProviderResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ApiBaseUrl { get; set; }
    public bool IsEnabled { get; set; }
}

public class ProductFulfillmentMappingResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid? ProductVariantId { get; set; }
    public string? VariantSku { get; set; }
    public Guid ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public string ExternalProductId { get; set; } = string.Empty;
    public string? ExternalVariantId { get; set; }
    public string ExternalSku { get; set; } = string.Empty;
    public string? DesignReference { get; set; }
    public string? DesignFileUrl { get; set; }
    public string? PrintingType { get; set; }
    public string? PrintingPlacement { get; set; }
    public bool IsActive { get; set; }
}

public class CreateFulfillmentMappingRequest
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public Guid ProviderId { get; set; }
    public string ExternalProductId { get; set; } = string.Empty;
    public string? ExternalVariantId { get; set; }
    public string ExternalSku { get; set; } = string.Empty;
    public string? DesignReference { get; set; }
    public string? DesignFileUrl { get; set; }
    public string? PrintingType { get; set; }
    public string? PrintingPlacement { get; set; }
}

public class UpdateFulfillmentMappingRequest
{
    public string? ExternalProductId { get; set; }
    public string? ExternalVariantId { get; set; }
    public string? ExternalSku { get; set; }
    public string? DesignReference { get; set; }
    public string? DesignFileUrl { get; set; }
    public string? PrintingType { get; set; }
    public string? PrintingPlacement { get; set; }
    public bool? IsActive { get; set; }
}
