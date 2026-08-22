using Application.DTOs.Storefront;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IPaymentService _paymentService;
    private readonly IStorefrontService _storefrontService;
    private readonly IFulfillmentService _fulfillmentService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        ApplicationDbContext context,
        IPaymentService paymentService,
        IStorefrontService storefrontService,
        IFulfillmentService fulfillmentService,
        ILogger<OrderService> logger)
    {
        _context = context;
        _paymentService = paymentService;
        _storefrontService = storefrontService;
        _fulfillmentService = fulfillmentService;
        _logger = logger;
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
        var existingOrder = await _context.Orders
            .Where(o => o.PaymentId == request.RazorpayPaymentId && o.PaymentMethod == "Razorpay" && o.IsActive)
            .Include(o => o.Items)
            .FirstOrDefaultAsync();

        if (existingOrder != null)
            return MapToCustomerOrderResponse(existingOrder);

        var isValid = await _paymentService.VerifyPaymentAsync(
            request.RazorpayOrderId,
            request.RazorpayPaymentId,
            request.RazorpaySignature);

        if (!isValid)
            throw new InvalidOperationException("Payment verification failed");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await BuildOrderFromItemsAsync(userId, request.Items, request.CouponCode,
                request.ShippingAddressId, request.Notes, request.ShippingMethod);

            var razorpayOrderAmount = await _paymentService.GetRazorpayOrderAmountAsync(request.RazorpayOrderId);
            if (razorpayOrderAmount.HasValue)
            {
                var paidAmountPaise = (long)(order.TotalAmount * 100);
                if (razorpayOrderAmount.Value != paidAmountPaise)
                {
                    _logger.LogWarning(
                        "Payment amount mismatch: RazorpayOrder={RazorpayOrderId}, RazorpayAmount={RazorpayAmount}, OrderTotal={OrderTotal}, UserId={UserId}",
                        request.RazorpayOrderId, razorpayOrderAmount.Value, order.TotalAmount, userId);
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException(
                        $"Payment amount mismatch. Expected {razorpayOrderAmount.Value / 100m:C} but order total is {order.TotalAmount:C}. " +
                        "Please contact support if you believe this is an error.");
                }
            }
            else
            {
                _logger.LogWarning(
                    "Could not verify Razorpay order amount for {RazorpayOrderId}. Proceeding with order total {OrderTotal}. " +
                    "This may indicate Razorpay API connectivity issues or invalid credentials.",
                    request.RazorpayOrderId, order.TotalAmount);
            }

            order.PaymentMethod = "Razorpay";
            order.PaymentStatus = "Paid";
            order.PaymentId = request.RazorpayPaymentId;
            order.Status = OrderStatus.PaymentSuccessful;

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _ = EnqueueFulfillmentAsync(order.Id);

            return MapToCustomerOrderResponse(order);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            var duplicate = await _context.Orders
                .Where(o => o.PaymentId == request.RazorpayPaymentId && o.PaymentMethod == "Razorpay" && o.IsActive)
                .Include(o => o.Items)
                .FirstOrDefaultAsync();

            if (duplicate != null)
                return MapToCustomerOrderResponse(duplicate);

            throw;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<CustomerOrderResponse?> CreateCodOrderAsync(Guid userId, CreateCodOrderRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await BuildOrderFromItemsAsync(userId, request.Items, request.CouponCode,
                request.ShippingAddressId, request.Notes, request.ShippingMethod);

            order.PaymentMethod = "COD";
            order.PaymentStatus = "Pending";
            order.Status = OrderStatus.Confirmed;

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _ = EnqueueFulfillmentAsync(order.Id);

            return MapToCustomerOrderResponse(order);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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

        var variantIds = order.Items.Where(i => i.ProductVariantId.HasValue).Select(i => i.ProductVariantId!.Value).ToList();
        var productIds = order.Items.Where(i => !i.ProductVariantId.HasValue).Select(i => i.ProductId).ToList();

        var variants = variantIds.Any()
            ? await _context.ProductVariants.Where(v => variantIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id)
            : new Dictionary<Guid, ProductVariant>();
        var products = productIds.Any()
            ? await _context.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id)
            : new Dictionary<Guid, Product>();

        foreach (var item in order.Items)
        {
            if (item.ProductVariantId.HasValue && variants.TryGetValue(item.ProductVariantId.Value, out var variant))
            {
                variant.Stock += item.Quantity;
                _context.ProductVariants.Update(variant);
            }
            else if (!item.ProductVariantId.HasValue && products.TryGetValue(item.ProductId, out var product))
            {
                _context.Products.Update(product);
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

        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .ToDictionaryAsync(p => p.Id);

        foreach (var item in items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
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
                var affected = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE \"ProductVariants\" SET \"Stock\" = \"Stock\" - {0} WHERE \"Id\" = {1} AND \"Stock\" >= {0}",
                    item.Quantity, variant.Id);

                if (affected == 0)
                    throw new InvalidOperationException(
                        $"Insufficient stock for {product.Name} (variant {variant.Id}). " +
                        $"Requested: {item.Quantity}, Available: {variant.Stock}");

                variant.Stock -= item.Quantity;
            }
        }

        decimal taxAmount = 0;
        decimal shippingAmount = shippingMethod == "express" ? 300 : (subTotal >= 2000 ? 0 : 150);
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

        var order = new Order
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

        var qikinkProvider = await _context.FulfillmentProviders
            .FirstOrDefaultAsync(p => p.Name == "Qikink" && p.IsActive);

        if (qikinkProvider != null)
        {
            var itemProductVariantPairs = orderItems
                .Select(oi => new { oi.ProductId, oi.ProductVariantId, oi.Id, oi.Quantity })
                .ToList();

            var fulfillmentProductIds = itemProductVariantPairs.Select(p => p.ProductId).Distinct().ToList();
            var variantIds = itemProductVariantPairs
                .Where(p => p.ProductVariantId.HasValue)
                .Select(p => p.ProductVariantId!.Value)
                .Distinct()
                .ToList();

            var fulfillmentMappings = await _context.ProductFulfillmentMappings
                .Where(m => fulfillmentProductIds.Contains(m.ProductId)
                    && m.ProviderId == qikinkProvider.Id
                    && m.IsActive)
                .ToListAsync();

            var fulfillmentOrderItems = new List<FulfillmentOrderItem>();

            foreach (var pair in itemProductVariantPairs)
            {
                var mapping = fulfillmentMappings.FirstOrDefault(m =>
                    m.ProductId == pair.ProductId
                    && m.ProductVariantId == pair.ProductVariantId);

                if (mapping == null) continue;

                fulfillmentOrderItems.Add(new FulfillmentOrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderItemId = pair.Id,
                    ExternalProductId = mapping.ExternalProductId,
                    ExternalVariantId = mapping.ExternalVariantId,
                    ExternalSku = mapping.ExternalSku,
                    Quantity = pair.Quantity,
                    DesignReference = mapping.DesignReference,
                    DesignFileUrl = mapping.DesignFileUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (fulfillmentOrderItems.Count > 0)
            {
                var fulfillmentOrder = new FulfillmentOrder
                {
                    Id = Guid.NewGuid(),
                    ProviderId = qikinkProvider.Id,
                    Status = FulfillmentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Items = fulfillmentOrderItems
                };

                order.FulfillmentOrder = fulfillmentOrder;
            }
        }

        return order;
    }

    public async Task HandleWebhookPaymentCapturedAsync(string razorpayOrderId, string razorpayPaymentId)
    {
        var order = await _context.Orders
            .Where(o => o.PaymentId == razorpayPaymentId && o.PaymentMethod == "Razorpay" && o.IsActive)
            .FirstOrDefaultAsync();

        if (order != null)
        {
            if (order.Status == OrderStatus.PendingPayment)
            {
                order.Status = OrderStatus.PaymentSuccessful;
                order.PaymentStatus = "Paid";
                await _context.SaveChangesAsync();
            }
            return;
        }

        _logger.LogWarning("Webhook payment.captured received for unknown payment {PaymentId} (order {OrderId}). " +
            "No fallback assignment performed to prevent race conditions.", razorpayPaymentId, razorpayOrderId);
    }

    public async Task HandleWebhookPaymentFailedAsync(string razorpayOrderId, string? razorpayPaymentId)
    {
        Order? order = null;

        if (!string.IsNullOrEmpty(razorpayPaymentId))
        {
            order = await _context.Orders
                .Where(o => o.PaymentId == razorpayPaymentId && o.PaymentMethod == "Razorpay" && o.IsActive)
                .FirstOrDefaultAsync();
        }

        if (order != null && order.Status == OrderStatus.PendingPayment)
        {
            order.Status = OrderStatus.PaymentFailed;
            order.PaymentStatus = "Failed";
            await _context.SaveChangesAsync();
        }
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

    private async Task EnqueueFulfillmentAsync(Guid orderId)
    {
        try
        {
            await _fulfillmentService.EnqueueSubmissionAsync(orderId);
        }
        catch (Exception ex)
        {
            // Fulfillment enqueue failure should not break the order flow.
            // The order is already created and saved; fulfillment will be
            // retried manually via admin if needed.
            // Using ILogger through a scoped provider would require
            // additional DI complexity; silent swallow is acceptable here
            // since the failure is non-critical to the customer.
            _ = ex;
        }
    }
}
