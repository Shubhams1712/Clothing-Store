using Application.DTOs.Admin;

namespace Application.Interfaces;

public interface IAnalyticsService
{
    Task<DashboardAnalyticsResponse> GetDashboardAnalyticsAsync(AnalyticsDateRange? dateRange);
    Task<SalesAnalyticsResponse> GetSalesAnalyticsAsync(AnalyticsDateRange? dateRange);
    Task<ProductAnalyticsResponse> GetProductAnalyticsAsync(AnalyticsDateRange? dateRange);
    Task<CustomerAnalyticsResponse> GetCustomerAnalyticsAsync(AnalyticsDateRange? dateRange);
    Task<InventoryAnalyticsResponse> GetInventoryAnalyticsAsync(AnalyticsDateRange? dateRange);
    Task<OrderAnalyticsResponse> GetOrderAnalyticsAsync(AnalyticsDateRange? dateRange);
    Task<byte[]> ExportReportAsync(ReportExportRequest request);
}
