using Application.DTOs.Storefront;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IPaymentService _paymentService;
    private readonly IStorefrontService _storefrontService;

    public OrderService(
        ApplicationDbContext context,
        IPaymentService paymentService,
        IStorefrontService storefrontService)
    {
        _context = context;
        _paymentService = paymentService;
        _storefrontService = storefrontService;
    }

    public async Task<List<CustomerOrderResponse>> GetUserOrdersAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var orders = await _context.Orders
            .Where(o => o.UserId == userId && o.IsActive)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return orders.Select(MapToCustomerOrderResponse).ToList();
    }

    public async Task<CustomerOrderResponse?> GetOrderByIdAsync(Guid userId, Guid orderId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.UserId == userId && o.IsActive)
            .Include(o => o.Items)
            .FirstOrDefaultAsync();

        return order == null ? null : MapToCustomerOrderResponse(order);
    }

    public async Task<CustomerOrderResponse> CreateOrderAsync(Guid userId, CreateOrderFromPaymentRequest request)
    {
        var isValid = await _paymentService.VerifyPaymentAsync(
            request.RazorpayOrderId,
            request.RazorpayPaymentId,
            request.RazorpaySignature);

        if (!isValid)
            throw new InvalidOperationException("Payment verification failed");

        var order = await BuildOrderFromItemsAsync(userId, request.Items, request.CouponCode,
            request.ShippingAddressId, request.Notes, request.ShippingMethod);

        order.PaymentMethod = "Razorpay";
        order.PaymentStatus = "Paid";
        order.PaymentId = request.RazorpayPaymentId;
        order.Status = OrderStatus.PaymentSuccessful;

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        return MapToCustomerOrderResponse(order);
    }

    public async Task<CustomerOrderResponse?> CreateCodOrderAsync(Guid userId, CreateCodOrderRequest request)
    {
        var order = await BuildOrderFromItemsAsync(userId, request.Items, request.CouponCode,
            request.ShippingAddressId, request.Notes, null);

        order.PaymentMethod = "COD";
        order.PaymentStatus = "Pending";
        order.Status = OrderStatus.PendingPayment;

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        return MapToCustomerOrderResponse(order);
    }

    public async Task<bool> CancelOrderAsync(Guid userId, Guid orderId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.UserId == userId && o.IsActive)
            .Include(o => o.Items)
            .FirstOrDefaultAsync();

        if (order == null) return false;

        if (order.Status == OrderStatus.Delivered ||
            order.Status == OrderStatus.Shipped ||
            order.Status == OrderStatus.OutForDelivery)
            return false;

        if (order.Status == OrderStatus.Cancelled)
            return false;

        order.Status = OrderStatus.Cancelled;

        foreach (var item in order.Items)
        {
            if (item.ProductVariantId.HasValue)
            {
                var variant = await _context.ProductVariants.FindAsync(item.ProductVariantId.Value);
                if (variant != null)
                {
                    variant.Stock += item.Quantity;
                    _context.ProductVariants.Update(variant);
                }
            }
            else
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    _context.Products.Update(product);
                }
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RequestRefundAsync(Guid userId, Guid orderId, string? reason)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.UserId == userId && o.IsActive)
            .FirstOrDefaultAsync();

        if (order == null) return false;

        if (order.Status != OrderStatus.Delivered)
            return false;

        order.Status = OrderStatus.RefundRequested;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<OrderTrackingResponse?> GetOrderTrackingAsync(Guid userId, Guid orderId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.UserId == userId && o.IsActive)
            .FirstOrDefaultAsync();

        if (order == null) return null;

        var timeline = new List<OrderTrackingEvent>
        {
            new() { Status = "Order Placed", Timestamp = order.CreatedAt, Description = "Your order has been placed" }
        };

        if (order.Status >= OrderStatus.PaymentSuccessful)
            timeline.Add(new() { Status = "Payment Confirmed", Timestamp = order.CreatedAt.AddSeconds(1), Description = "Payment has been verified" });

        if (order.Status >= OrderStatus.Confirmed)
            timeline.Add(new() { Status = "Order Confirmed", Timestamp = order.CreatedAt.AddMinutes(5), Description = "Order has been confirmed" });

        if (order.Status >= OrderStatus.Packed)
            timeline.Add(new() { Status = "Packed", Timestamp = order.CreatedAt.AddHours(2), Description = "Order has been packed" });

        if (order.Status >= OrderStatus.Shipped && order.ShippedAt.HasValue)
            timeline.Add(new() { Status = "Shipped", Timestamp = order.ShippedAt.Value, Description = "Order has been shipped" });

        if (order.Status >= OrderStatus.OutForDelivery && order.ShippedAt.HasValue)
            timeline.Add(new() { Status = "Out for Delivery", Timestamp = order.ShippedAt.Value.AddHours(6), Description = "Order is out for delivery" });

        if (order.Status >= OrderStatus.Delivered && order.DeliveredAt.HasValue)
            timeline.Add(new() { Status = "Delivered", Timestamp = order.DeliveredAt.Value, Description = "Order has been delivered" });

        if (order.Status == OrderStatus.Cancelled)
            timeline.Add(new() { Status = "Cancelled", Timestamp = order.UpdatedAt, Description = "Order has been cancelled" });

        if (order.Status == OrderStatus.RefundRequested)
            timeline.Add(new() { Status = "Refund Requested", Timestamp = order.UpdatedAt, Description = "Refund request has been submitted" });

        if (order.Status == OrderStatus.Refunded)
            timeline.Add(new() { Status = "Refunded", Timestamp = order.UpdatedAt, Description = "Refund has been processed" });

        return new OrderTrackingResponse
        {
            OrderNumber = order.OrderNumber,
            CurrentStatus = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            Timeline = timeline
        };
    }

    public async Task<InvoiceResponse?> GetOrderInvoiceAsync(Guid userId, Guid orderId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.UserId == userId && o.IsActive)
            .Include(o => o.Items)
            .FirstOrDefaultAsync();

        if (order == null) return null;

        var user = await _context.Users.FindAsync(userId);

        return new InvoiceResponse
        {
            OrderNumber = order.OrderNumber,
            InvoiceDate = order.CreatedAt,
            CustomerName = user != null ? $"{user.FirstName} {user.LastName}" : "",
            CustomerEmail = user?.Email ?? "",
            ShippingName = order.ShippingName ?? "",
            ShippingAddress = order.ShippingAddress,
            ShippingCity = order.ShippingCity,
            ShippingState = order.ShippingState,
            ShippingPostalCode = order.ShippingPostalCode,
            ShippingCountry = order.ShippingCountry,
            Items = order.Items.Select(i => new InvoiceItemResponse
            {
                ProductName = i.ProductName,
                Sku = i.Sku,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList(),
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            ShippingAmount = order.ShippingAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod,
            PaymentId = order.PaymentId
        };
    }

    private async Task<Order> BuildOrderFromItemsAsync(
        Guid userId,
        List<CheckoutItemRequest> items,
        string? couponCode,
        Guid? shippingAddressId,
        string? notes,
        string? shippingMethod)
    {
        var orderItems = new List<OrderItem>();
        decimal subTotal = 0;

        foreach (var item in items)
        {
            var product = await _context.Products
                .Where(p => p.Id == item.ProductId && p.IsActive)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .FirstOrDefaultAsync();

            if (product == null)
                throw new InvalidOperationException($"Product {item.ProductId} not found");

            var variant = item.VariantId.HasValue
                ? product.Variants.FirstOrDefault(v => v.Id == item.VariantId.Value && v.IsActive)
                : null;

            var unitPrice = variant?.Price ?? product.Price;
            var sku = variant?.Sku ?? product.Sku;
            var imageUrl = product.Images.FirstOrDefault(i => i.IsActive && i.IsFeatured)?.Url
                ?? product.Images.FirstOrDefault(i => i.IsActive)?.Url;

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                ProductVariantId = variant?.Id,
                ProductName = product.Name,
                Sku = sku,
                ImageUrl = imageUrl,
                Size = variant?.Size,
                Color = variant?.Color,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = unitPrice * item.Quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            orderItems.Add(orderItem);
            subTotal += orderItem.TotalPrice;

            if (variant != null)
            {
                variant.Stock -= item.Quantity;
                if (variant.Stock < 0) variant.Stock = 0;
                _context.ProductVariants.Update(variant);
            }
        }

        decimal taxAmount = Math.Round(subTotal * 0.18m, 2);
        decimal shippingAmount = subTotal >= 2000 ? 0 : 150;
        decimal discountAmount = 0;

        if (!string.IsNullOrEmpty(couponCode))
        {
            var couponResult = await _storefrontService.ApplyCouponAsync(new ApplyCouponRequest
            {
                Code = couponCode,
                OrderSubtotal = subTotal
            });
            if (couponResult.IsValid)
                discountAmount = couponResult.DiscountAmount;
        }

        decimal totalAmount = subTotal + taxAmount + shippingAmount - discountAmount;
        if (totalAmount < 0) totalAmount = 0;

        Address? shippingAddress = null;
        if (shippingAddressId.HasValue)
        {
            shippingAddress = await _context.Addresses
                .Where(a => a.Id == shippingAddressId.Value && a.UserId == userId && a.IsActive)
                .FirstOrDefaultAsync();
        }

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        return new Order
        {
            OrderNumber = orderNumber,
            UserId = userId,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            ShippingAmount = shippingAmount,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            Currency = "INR",
            ShippingName = shippingAddress?.FullName,
            ShippingAddress = shippingAddress?.AddressLine1,
            ShippingCity = shippingAddress?.City,
            ShippingState = shippingAddress?.State,
            ShippingPostalCode = shippingAddress?.PostalCode,
            ShippingCountry = shippingAddress?.Country,
            ShippingPhone = shippingAddress?.Phone,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = orderItems
        };
    }

    private static CustomerOrderResponse MapToCustomerOrderResponse(Order order)
    {
        return new CustomerOrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            ShippingAmount = order.ShippingAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            PaymentId = order.PaymentId,
            ShippingName = order.ShippingName,
            ShippingAddress = order.ShippingAddress,
            ShippingCity = order.ShippingCity,
            ShippingState = order.ShippingState,
            ShippingPostalCode = order.ShippingPostalCode,
            ShippingCountry = order.ShippingCountry,
            ShippingPhone = order.ShippingPhone,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            Items = order.Items.Select(i => new CustomerOrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Sku = i.Sku,
                ImageUrl = i.ImageUrl,
                Size = i.Size,
                Color = i.Color,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }
}
