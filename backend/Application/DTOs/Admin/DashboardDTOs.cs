namespace Application.DTOs.Admin;

public class DashboardStatsResponse
{
    public decimal TotalRevenue { get; set; }
    public int TodayOrders { get; set; }
    public int PendingOrders { get; set; }
    public int LowStockProducts { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalOrders { get; set; }
    public List<OrderResponse> RecentOrders { get; set; } = new();
    public List<ProductBriefResponse> TopSellingProducts { get; set; } = new();
    public List<CustomerResponse> LatestCustomers { get; set; } = new();
    public List<SalesOverviewPoint> SalesOverview { get; set; } = new();
}

public class SalesOverviewPoint
{
    public string Date { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}
