namespace Application.Interfaces;

public interface IAnalyticsService
{
    Task<object?> GetDashboardSummaryAsync(DateTime? startDate, DateTime? endDate);
    Task<object?> GetSalesAnalyticsAsync(DateTime? startDate, DateTime? endDate);
    Task<object?> GetProductAnalyticsAsync(DateTime? startDate, DateTime? endDate);
    Task<object?> GetCustomerAnalyticsAsync(DateTime? startDate, DateTime? endDate);
}
