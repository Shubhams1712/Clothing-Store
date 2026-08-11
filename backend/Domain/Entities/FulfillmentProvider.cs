using Domain.Common;

namespace Domain.Entities;

public class FulfillmentProvider : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ApiBaseUrl { get; set; }
    public bool IsEnabled { get; set; } = true;
    public ICollection<FulfillmentOrder> FulfillmentOrders { get; set; } = new List<FulfillmentOrder>();
    public ICollection<ProductFulfillmentMapping> ProductMappings { get; set; } = new List<ProductFulfillmentMapping>();
}
