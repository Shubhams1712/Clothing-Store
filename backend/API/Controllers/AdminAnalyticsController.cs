using Application.Common.Models;
using Application.DTOs.Admin;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "Admin,Manager")]
[EnableRateLimiting("global")]
public class AdminAnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AdminAnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardAnalyticsResponse>>> GetDashboardAnalytics([FromQuery] AnalyticsDateRange? dateRange)
    {
        var result = await _analyticsService.GetDashboardAnalyticsAsync(dateRange);
        return Ok(ApiResponse<DashboardAnalyticsResponse>.SuccessResponse(result));
    }

    [HttpGet("sales")]
    public async Task<ActionResult<ApiResponse<SalesAnalyticsResponse>>> GetSalesAnalytics([FromQuery] AnalyticsDateRange? dateRange)
    {
        var result = await _analyticsService.GetSalesAnalyticsAsync(dateRange);
        return Ok(ApiResponse<SalesAnalyticsResponse>.SuccessResponse(result));
    }

    [HttpGet("products")]
    public async Task<ActionResult<ApiResponse<ProductAnalyticsResponse>>> GetProductAnalytics([FromQuery] AnalyticsDateRange? dateRange)
    {
        var result = await _analyticsService.GetProductAnalyticsAsync(dateRange);
        return Ok(ApiResponse<ProductAnalyticsResponse>.SuccessResponse(result));
    }

    [HttpGet("customers")]
    public async Task<ActionResult<ApiResponse<CustomerAnalyticsResponse>>> GetCustomerAnalytics([FromQuery] AnalyticsDateRange? dateRange)
    {
        var result = await _analyticsService.GetCustomerAnalyticsAsync(dateRange);
        return Ok(ApiResponse<CustomerAnalyticsResponse>.SuccessResponse(result));
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<ApiResponse<InventoryAnalyticsResponse>>> GetInventoryAnalytics([FromQuery] AnalyticsDateRange? dateRange)
    {
        var result = await _analyticsService.GetInventoryAnalyticsAsync(dateRange);
        return Ok(ApiResponse<InventoryAnalyticsResponse>.SuccessResponse(result));
    }

    [HttpGet("orders")]
    public async Task<ActionResult<ApiResponse<OrderAnalyticsResponse>>> GetOrderAnalytics([FromQuery] AnalyticsDateRange? dateRange)
    {
        var result = await _analyticsService.GetOrderAnalyticsAsync(dateRange);
        return Ok(ApiResponse<OrderAnalyticsResponse>.SuccessResponse(result));
    }

    [HttpPost("reports/export")]
    public async Task<IActionResult> ExportReport([FromBody] ReportExportRequest request)
    {
        var result = await _analyticsService.ExportReportAsync(request);
        var contentType = request.Format.ToLowerInvariant() switch
        {
            "csv" => "text/csv",
            "json" => "application/json",
            _ => "text/csv"
        };
        var fileName = $"{request.ReportType}-report-{DateTime.UtcNow:yyyyMMdd}.{request.Format}";
        return File(result, contentType, fileName);
    }
}
