using System.Globalization;
using System.Text;
using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _context;

    public AnalyticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardAnalyticsResponse> GetDashboardAnalyticsAsync(AnalyticsDateRange? dateRange)
    {
        var (startDate, endDate) = GetDateRange(dateRange);

        var orders = _context.Orders.Where(o => o.IsActive);
        var customers = _context.Users.Where(u => u.IsActive);
        var products = _context.Products.Where(p => p.IsActive);
        var variants = _context.ProductVariants.Where(v => v.IsActive);

        if (startDate.HasValue)
            orders = orders.Where(o => o.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            orders = orders.Where(o => o.CreatedAt <= endDate.Value);

        var totalRevenue = await orders
            .Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded && o.Status != OrderStatus.PaymentFailed)
            .SumAsync(o => o.TotalAmount);

        var totalOrders = await orders.CountAsync();
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        var totalCustomersCount = await customers.CountAsync();
        var newCustomersCount = startDate.HasValue
            ? await customers.CountAsync(u => u.CreatedAt >= startDate.Value)
            : totalCustomersCount;

        var refunds = await orders
            .Where(o => o.Status == OrderStatus.Refunded)
            .SumAsync(o => o.TotalAmount);

        var pendingOrders = await orders.CountAsync(o => o.Status == OrderStatus.PendingPayment);

        var lowStockProducts = await variants.CountAsync(v => v.Stock > 0 && v.Stock <= 5);

        var revenueOverTime = await orders
            .Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded && o.Status != OrderStatus.PaymentFailed)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailyRevenuePoint
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Revenue = g.Sum(o => o.TotalAmount),
                Orders = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return new DashboardAnalyticsResponse
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            AverageOrderValue = averageOrderValue,
            TotalCustomers = totalCustomersCount,
            NewCustomers = newCustomersCount,
            Refunds = refunds,
            PendingOrders = pendingOrders,
            LowStockProducts = lowStockProducts,
            ConversionRate = 0,
            RevenueOverTime = revenueOverTime
        };
    }

    public async Task<SalesAnalyticsResponse> GetSalesAnalyticsAsync(AnalyticsDateRange? dateRange)
    {
        var (startDate, endDate) = GetDateRange(dateRange);

        var orders = _context.Orders.Where(o => o.IsActive);
        if (startDate.HasValue) orders = orders.Where(o => o.CreatedAt >= startDate.Value);
        if (endDate.HasValue) orders = orders.Where(o => o.CreatedAt <= endDate.Value);

        var paidOrders = orders.Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded && o.Status != OrderStatus.PaymentFailed);

        var totalRevenue = await paidOrders.SumAsync(o => o.TotalAmount);
        var totalOrdersCount = await paidOrders.CountAsync();
        var averageOrderValue = totalOrdersCount > 0 ? totalRevenue / totalOrdersCount : 0;
        var totalDiscounts = await paidOrders.SumAsync(o => o.DiscountAmount);
        var totalRefunds = await orders.Where(o => o.Status == OrderStatus.Refunded).SumAsync(o => o.TotalAmount);

        var revenueOverTime = await paidOrders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailyRevenuePoint
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Revenue = g.Sum(o => o.TotalAmount),
                Orders = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var revenueByPaymentMethod = await paidOrders
            .GroupBy(o => o.PaymentMethod ?? "Unknown")
            .Select(g => new PaymentMethodBreakdown
            {
                PaymentMethod = g.Key,
                Revenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .ToListAsync();

        var orderItems = _context.OrderItems.Where(oi => oi.IsActive);
        if (startDate.HasValue) orderItems = orderItems.Where(oi => oi.CreatedAt >= startDate.Value);
        if (endDate.HasValue) orderItems = orderItems.Where(oi => oi.CreatedAt <= endDate.Value);

        var revenueByCategory = await orderItems
            .Join(_context.Products.Where(p => p.IsActive), oi => oi.ProductId, p => p.Id, (oi, p) => new { oi, p })
            .GroupBy(x => x.p.Category != null ? x.p.Category.Name : "Uncategorized")
            .Select(g => new CategoryRevenue
            {
                CategoryName = g.Key,
                Revenue = g.Sum(x => x.oi.TotalPrice),
                OrderCount = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .ToListAsync();

        var coupons = _context.Coupons.Where(c => c.IsActive);
        var topCoupons = await coupons
            .OrderByDescending(c => c.UsedCount)
            .Take(10)
            .Select(c => new CouponUsage
            {
                Code = c.Code,
                UsedCount = c.UsedCount,
                TotalDiscount = c.Value * c.UsedCount
            })
            .ToListAsync();

        return new SalesAnalyticsResponse
        {
            TotalRevenue = totalRevenue,
            AverageOrderValue = averageOrderValue,
            TotalOrders = totalOrdersCount,
            TotalDiscounts = totalDiscounts,
            TotalRefunds = totalRefunds,
            RevenueOverTime = revenueOverTime,
            RevenueByPaymentMethod = revenueByPaymentMethod,
            RevenueByCategory = revenueByCategory,
            RevenueByCollection = new List<CollectionRevenue>(),
            TopCoupons = topCoupons
        };
    }

    public async Task<ProductAnalyticsResponse> GetProductAnalyticsAsync(AnalyticsDateRange? dateRange)
    {
        var (startDate, endDate) = GetDateRange(dateRange);

        var orderItems = _context.OrderItems.Where(oi => oi.IsActive);
        if (startDate.HasValue) orderItems = orderItems.Where(oi => oi.CreatedAt >= startDate.Value);
        if (endDate.HasValue) orderItems = orderItems.Where(oi => oi.CreatedAt <= endDate.Value);

        var productSales = await orderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalSold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(x => x.Revenue)
            .ToListAsync();

        var productIds = productSales.Select(x => x.ProductId).ToList();
        var productsDict = await _context.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id);

        var bestSelling = productSales.Take(10).Select(x => new ProductSalesInfo
        {
            ProductId = x.ProductId,
            ProductName = productsDict.TryGetValue(x.ProductId, out var p) ? p.Name : "Unknown",
            ImageUrl = productsDict.TryGetValue(x.ProductId, out var pImg) ? pImg.Images.FirstOrDefault(i => i.IsActive && i.IsFeatured)?.Url : null,
            TotalSold = x.TotalSold,
            Revenue = x.Revenue
        }).ToList();

        var worstPerforming = productSales.Where(x => x.TotalSold > 0).OrderBy(x => x.Revenue).Take(10).Select(x => new ProductSalesInfo
        {
            ProductId = x.ProductId,
            ProductName = productsDict.TryGetValue(x.ProductId, out var p2) ? p2.Name : "Unknown",
            ImageUrl = productsDict.TryGetValue(x.ProductId, out var pImg2) ? pImg2.Images.FirstOrDefault(i => i.IsActive && i.IsFeatured)?.Url : null,
            TotalSold = x.TotalSold,
            Revenue = x.Revenue
        }).ToList();

        var variants = await _context.ProductVariants
            .Where(v => v.IsActive)
            .Include(v => v.Product)
            .ToListAsync();

        var lowStock = variants.Where(v => v.Stock > 0 && v.Stock <= 5)
            .GroupBy(v => v.ProductId)
            .Select(g => new ProductStockInfo
            {
                ProductId = g.Key,
                ProductName = g.First().Product?.Name ?? "Unknown",
                ImageUrl = g.First().Product?.Images.FirstOrDefault(i => i.IsActive && i.IsFeatured)?.Url,
                TotalStock = g.Sum(v => v.Stock),
                VariantCount = g.Count()
            })
            .ToList();

        var outOfStock = variants.Where(v => v.Stock == 0)
            .GroupBy(v => v.ProductId)
            .Select(g => new ProductStockInfo
            {
                ProductId = g.Key,
                ProductName = g.First().Product?.Name ?? "Unknown",
                ImageUrl = g.First().Product?.Images.FirstOrDefault(i => i.IsActive && i.IsFeatured)?.Url,
                TotalStock = 0,
                VariantCount = g.Count()
            })
            .ToList();

        return new ProductAnalyticsResponse
        {
            BestSelling = bestSelling,
            WorstPerforming = worstPerforming,
            LowStock = lowStock,
            OutOfStock = outOfStock
        };
    }

    public async Task<CustomerAnalyticsResponse> GetCustomerAnalyticsAsync(AnalyticsDateRange? dateRange)
    {
        var (startDate, endDate) = GetDateRange(dateRange);

        var customers = _context.Users.Where(u => u.IsActive);
        var orders = _context.Orders.Where(o => o.IsActive);

        var totalCustomers = await customers.CountAsync();
        var newCustomers = startDate.HasValue
            ? await customers.CountAsync(u => u.CreatedAt >= startDate.Value)
            : totalCustomers;

        var customerOrderCounts = await orders
            .GroupBy(o => o.UserId)
            .Select(g => new { UserId = g.Key, OrderCount = g.Count(), TotalSpent = g.Sum(o => o.TotalAmount) })
            .ToListAsync();

        var returningCustomers = customerOrderCounts.Count(c => c.OrderCount > 1);
        var repeatPurchaseRate = totalCustomers > 0 ? (decimal)returningCustomers / totalCustomers * 100 : 0;
        var averageLifetimeValue = customerOrderCounts.Any() ? customerOrderCounts.Average(c => c.TotalSpent) : 0;

        var customerGrowth = await customers
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new CustomerGrowthPoint
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                NewCustomers = g.Count(),
                TotalCustomers = 0
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var runningTotal = 0;
        foreach (var point in customerGrowth)
        {
            runningTotal += point.NewCustomers;
            point.TotalCustomers = runningTotal;
        }

        var topCustomers = customerOrderCounts
            .OrderByDescending(c => c.TotalSpent)
            .Take(10)
            .ToList();

        var topCustomerIds = topCustomers.Select(c => c.UserId).ToList();
        var topCustomerUsers = await _context.Users
            .Where(u => topCustomerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var topCustomerInfos = topCustomers.Select(c => new TopCustomerInfo
        {
            CustomerId = c.UserId,
            CustomerName = topCustomerUsers.TryGetValue(c.UserId, out var u) ? $"{u.FirstName} {u.LastName}" : "Unknown",
            Email = topCustomerUsers.TryGetValue(c.UserId, out var u2) ? u2.Email : "",
            OrderCount = c.OrderCount,
            TotalSpent = c.TotalSpent
        }).ToList();

        return new CustomerAnalyticsResponse
        {
            TotalCustomers = totalCustomers,
            NewCustomers = newCustomers,
            ReturningCustomers = returningCustomers,
            RepeatPurchaseRate = repeatPurchaseRate,
            AverageLifetimeValue = averageLifetimeValue,
            CustomerGrowth = customerGrowth,
            TopCustomers = topCustomerInfos
        };
    }

    public async Task<InventoryAnalyticsResponse> GetInventoryAnalyticsAsync(AnalyticsDateRange? dateRange)
    {
        var variants = await _context.ProductVariants
            .Where(v => v.IsActive)
            .Include(v => v.Product)
            .ToListAsync();

        var totalInventoryValue = variants.Sum(v => v.Price * v.Stock);
        var totalProducts = variants.Select(v => v.ProductId).Distinct().Count();
        var totalVariants = variants.Count;
        var lowStockCount = variants.Count(v => v.Stock > 0 && v.Stock <= 5);
        var outOfStockCount = variants.Count(v => v.Stock == 0);

        var fastMoving = await _context.OrderItems
            .Where(oi => oi.IsActive)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(oi => oi.Quantity) })
            .OrderByDescending(x => x.TotalSold)
            .Take(10)
            .ToListAsync();

        var fastMovingIds = fastMoving.Select(x => x.ProductId).ToList();
        var fastMovingProducts = await _context.Products
            .Where(p => fastMovingIds.Contains(p.Id) && p.IsActive)
            .Include(p => p.Variants.Where(v => v.IsActive))
            .ToDictionaryAsync(p => p.Id);

        var fastMovingProductsList = fastMoving.Select(x => new FastMovingProduct
        {
            ProductId = x.ProductId,
            ProductName = fastMovingProducts.TryGetValue(x.ProductId, out var p) ? p.Name : "Unknown",
            TotalSold = x.TotalSold,
            CurrentStock = fastMovingProducts.TryGetValue(x.ProductId, out var p2) ? p2.Variants.Sum(v => v.Stock) : 0
        }).ToList();

        var stockAlerts = variants.Where(v => v.Stock <= 5)
            .Select(v => new StockAlert
            {
                VariantId = v.Id,
                ProductName = v.Product?.Name ?? "Unknown",
                Sku = v.Sku,
                Size = v.Size,
                Color = v.Color,
                Stock = v.Stock
            })
            .ToList();

        return new InventoryAnalyticsResponse
        {
            TotalInventoryValue = totalInventoryValue,
            TotalProducts = totalProducts,
            TotalVariants = totalVariants,
            LowStockCount = lowStockCount,
            OutOfStockCount = outOfStockCount,
            FastMoving = fastMovingProductsList,
            SlowMoving = new List<SlowMovingProduct>(),
            StockAlerts = stockAlerts
        };
    }

    public async Task<OrderAnalyticsResponse> GetOrderAnalyticsAsync(AnalyticsDateRange? dateRange)
    {
        var (startDate, endDate) = GetDateRange(dateRange);

        var orders = _context.Orders.Where(o => o.IsActive);
        if (startDate.HasValue) orders = orders.Where(o => o.CreatedAt >= startDate.Value);
        if (endDate.HasValue) orders = orders.Where(o => o.CreatedAt <= endDate.Value);

        var totalOrders = await orders.CountAsync();
        var cancelledOrders = await orders.CountAsync(o => o.Status == OrderStatus.Cancelled);
        var refundedOrders = await orders.CountAsync(o => o.Status == OrderStatus.Refunded);

        var ordersByStatus = await orders
            .GroupBy(o => o.Status)
            .Select(g => new OrderStatusBreakdown
            {
                Status = g.Key.ToString(),
                Count = g.Count(),
                Percentage = totalOrders > 0 ? (decimal)g.Count() / totalOrders * 100 : 0
            })
            .ToListAsync();

        var ordersOverTime = await orders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailyRevenuePoint
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Revenue = g.Sum(o => o.TotalAmount),
                Orders = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return new OrderAnalyticsResponse
        {
            TotalOrders = totalOrders,
            CancelledOrders = cancelledOrders,
            RefundedOrders = refundedOrders,
            AverageFulfillmentTime = 0,
            OrdersByStatus = ordersByStatus,
            OrdersOverTime = ordersOverTime
        };
    }

    public async Task<byte[]> ExportReportAsync(ReportExportRequest request)
    {
        var (startDate, endDate) = GetDateRange(new AnalyticsDateRange
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate
        });

        var sb = new StringBuilder();

        switch (request.ReportType.ToLowerInvariant())
        {
            case "sales":
                var sales = await GetSalesAnalyticsAsync(new AnalyticsDateRange { StartDate = startDate, EndDate = endDate });
                sb.AppendLine("Date,Revenue,Orders");
                foreach (var point in sales.RevenueOverTime)
                    sb.AppendLine($"{point.Date},{point.Revenue},{point.Orders}");
                break;

            case "orders":
                var orderAnalytics = await GetOrderAnalyticsAsync(new AnalyticsDateRange { StartDate = startDate, EndDate = endDate });
                sb.AppendLine("Status,Count,Percentage");
                foreach (var status in orderAnalytics.OrdersByStatus)
                    sb.AppendLine($"{status.Status},{status.Count},{status.Percentage}");
                break;

            case "products":
                var productAnalytics = await GetProductAnalyticsAsync(new AnalyticsDateRange { StartDate = startDate, EndDate = endDate });
                sb.AppendLine("ProductId,ProductName,TotalSold,Revenue");
                foreach (var product in productAnalytics.BestSelling)
                    sb.AppendLine($"{product.ProductId},{product.ProductName},{product.TotalSold},{product.Revenue}");
                break;

            case "customers":
                var customerAnalytics = await GetCustomerAnalyticsAsync(new AnalyticsDateRange { StartDate = startDate, EndDate = endDate });
                sb.AppendLine("CustomerId,Name,Email,Orders,TotalSpent");
                foreach (var customer in customerAnalytics.TopCustomers)
                    sb.AppendLine($"{customer.CustomerId},{customer.CustomerName},{customer.Email},{customer.OrderCount},{customer.TotalSpent}");
                break;

            case "inventory":
                var inventoryAnalytics = await GetInventoryAnalyticsAsync(new AnalyticsDateRange { StartDate = startDate, EndDate = endDate });
                sb.AppendLine("ProductName,SKU,Size,Color,Stock");
                foreach (var alert in inventoryAnalytics.StockAlerts)
                    sb.AppendLine($"{alert.ProductName},{alert.Sku},{alert.Size ?? ""},{alert.Color ?? ""},{alert.Stock}");
                break;

            default:
                sb.AppendLine("Report type not supported");
                break;
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static (DateTime? StartDate, DateTime? EndDate) GetDateRange(AnalyticsDateRange? dateRange)
    {
        if (dateRange == null)
            return (null, null);

        if (dateRange.StartDate.HasValue && dateRange.EndDate.HasValue)
            return (dateRange.StartDate.Value, dateRange.EndDate.Value);

        if (!string.IsNullOrEmpty(dateRange.Preset))
        {
            var endDate = DateTime.UtcNow;
            DateTime startDate = dateRange.Preset.ToLowerInvariant() switch
            {
                "today" => endDate.Date,
                "yesterday" => endDate.Date.AddDays(-1),
                "7d" or "last7days" => endDate.AddDays(-7),
                "30d" or "last30days" => endDate.AddDays(-30),
                "90d" or "last90days" => endDate.AddDays(-90),
                "12m" or "last12months" => endDate.AddMonths(-12),
                "thismonth" => new DateTime(endDate.Year, endDate.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                "lastmonth" => new DateTime(endDate.Year, endDate.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1),
                _ => endDate.AddDays(-30)
            };
            return (startDate, endDate);
        }

        return (dateRange.StartDate, dateRange.EndDate);
    }
}
