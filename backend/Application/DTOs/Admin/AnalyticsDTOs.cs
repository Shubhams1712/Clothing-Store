namespace Application.DTOs.Admin;

public class AnalyticsDateRange
{
    public string? Preset { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class DashboardAnalyticsResponse
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalCustomers { get; set; }
    public int NewCustomers { get; set; }
    public decimal Refunds { get; set; }
    public int PendingOrders { get; set; }
    public int LowStockProducts { get; set; }
    public decimal ConversionRate { get; set; }
    public List<DailyRevenuePoint> RevenueOverTime { get; set; } = new();
}

public class DailyRevenuePoint
{
    public string Date { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}

public class SalesAnalyticsResponse
{
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal TotalRefunds { get; set; }
    public List<DailyRevenuePoint> RevenueOverTime { get; set; } = new();
    public List<PaymentMethodBreakdown> RevenueByPaymentMethod { get; set; } = new();
    public List<CategoryRevenue> RevenueByCategory { get; set; } = new();
    public List<CollectionRevenue> RevenueByCollection { get; set; } = new();
    public List<CouponUsage> TopCoupons { get; set; } = new();
}

public class PaymentMethodBreakdown
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class CategoryRevenue
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class CollectionRevenue
{
    public string CollectionName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class CouponUsage
{
    public string Code { get; set; } = string.Empty;
    public int UsedCount { get; set; }
    public decimal TotalDiscount { get; set; }
}

public class ProductAnalyticsResponse
{
    public List<ProductSalesInfo> BestSelling { get; set; } = new();
    public List<ProductSalesInfo> WorstPerforming { get; set; } = new();
    public List<ProductStockInfo> LowStock { get; set; } = new();
    public List<ProductStockInfo> OutOfStock { get; set; } = new();
}

public class ProductSalesInfo
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int TotalSold { get; set; }
    public decimal Revenue { get; set; }
}

public class ProductStockInfo
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int TotalStock { get; set; }
    public int VariantCount { get; set; }
}

public class CustomerAnalyticsResponse
{
    public int TotalCustomers { get; set; }
    public int NewCustomers { get; set; }
    public int ReturningCustomers { get; set; }
    public decimal RepeatPurchaseRate { get; set; }
    public decimal AverageLifetimeValue { get; set; }
    public List<CustomerGrowthPoint> CustomerGrowth { get; set; } = new();
    public List<TopCustomerInfo> TopCustomers { get; set; } = new();
}

public class CustomerGrowthPoint
{
    public string Date { get; set; } = string.Empty;
    public int NewCustomers { get; set; }
    public int TotalCustomers { get; set; }
}

public class TopCustomerInfo
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
}

public class InventoryAnalyticsResponse
{
    public decimal TotalInventoryValue { get; set; }
    public int TotalProducts { get; set; }
    public int TotalVariants { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public List<FastMovingProduct> FastMoving { get; set; } = new();
    public List<SlowMovingProduct> SlowMoving { get; set; } = new();
    public List<StockAlert> StockAlerts { get; set; } = new();
}

public class FastMovingProduct
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalSold { get; set; }
    public int CurrentStock { get; set; }
}

public class SlowMovingProduct
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalSold { get; set; }
    public int CurrentStock { get; set; }
}

public class StockAlert
{
    public Guid VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Color { get; set; }
    public int Stock { get; set; }
}

public class OrderAnalyticsResponse
{
    public int TotalOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int RefundedOrders { get; set; }
    public double AverageFulfillmentTime { get; set; }
    public List<OrderStatusBreakdown> OrdersByStatus { get; set; } = new();
    public List<DailyRevenuePoint> OrdersOverTime { get; set; } = new();
}

public class OrderStatusBreakdown
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class ReportExportRequest
{
    public string ReportType { get; set; } = string.Empty;
    public string Format { get; set; } = "csv";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
