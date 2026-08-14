using System.Threading.Channels;
using Application.DTOs.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class FulfillmentService : IFulfillmentService
{
    private readonly Channel<Guid> _channel;
    private readonly ApplicationDbContext _context;

    public FulfillmentService(Channel<Guid> channel, ApplicationDbContext context)
    {
        _channel = channel;
        _context = context;
    }

    public async Task EnqueueSubmissionAsync(Guid orderId)
    {
        await _channel.Writer.WriteAsync(orderId);
    }

    public Task<object?> SubmitOrderAsync(Guid orderId)
    {
        return Task.FromResult<object?>(null);
    }

    public Task<object?> GetOrderStatusAsync(Guid orderId)
    {
        return Task.FromResult<object?>(null);
    }

    public async Task<FulfillmentOrderResponse?> GetFulfillmentOrderAsync(Guid orderId)
    {
        var fulfillmentOrder = await _context.FulfillmentOrders
            .Where(fo => fo.OrderId == orderId && fo.IsActive)
            .Include(fo => fo.Items.Where(i => i.IsActive))
            .Include(fo => fo.Shipment)
            .Include(fo => fo.Provider)
            .Include(fo => fo.Order)
            .FirstOrDefaultAsync();

        return fulfillmentOrder == null ? null : MapToFulfillmentOrderResponse(fulfillmentOrder);
    }

    public async Task<PaginatedResponse<FulfillmentOrderResponse>> GetFulfillmentOrdersAsync(PaginatedRequest request)
    {
        var query = _context.FulfillmentOrders
            .Where(fo => fo.IsActive)
            .Include(fo => fo.Items.Where(i => i.IsActive))
            .Include(fo => fo.Shipment)
            .Include(fo => fo.Provider)
            .Include(fo => fo.Order)
            .OrderByDescending(fo => fo.CreatedAt)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PaginatedResponse<FulfillmentOrderResponse>
        {
            Items = items.Select(MapToFulfillmentOrderResponse).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<List<FulfillmentProviderResponse>> GetProvidersAsync()
    {
        return await _context.FulfillmentProviders
            .Where(p => p.IsActive)
            .Select(p => new FulfillmentProviderResponse
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                ApiBaseUrl = p.ApiBaseUrl,
                IsEnabled = p.IsEnabled
            })
            .ToListAsync();
    }

    public async Task<PaginatedResponse<ProductFulfillmentMappingResponse>> GetMappingsAsync(PaginatedRequest request)
    {
        var query = _context.ProductFulfillmentMappings
            .Where(m => m.IsActive)
            .Include(m => m.Product)
            .Include(m => m.ProductVariant)
            .Include(m => m.Provider)
            .OrderByDescending(m => m.CreatedAt)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PaginatedResponse<ProductFulfillmentMappingResponse>
        {
            Items = items.Select(m => new ProductFulfillmentMappingResponse
            {
                Id = m.Id,
                ProductId = m.ProductId,
                ProductName = m.Product?.Name,
                ProductVariantId = m.ProductVariantId,
                VariantSku = m.ProductVariant?.Sku,
                ProviderId = m.ProviderId,
                ProviderName = m.Provider?.Name,
                ExternalProductId = m.ExternalProductId,
                ExternalVariantId = m.ExternalVariantId,
                ExternalSku = m.ExternalSku,
                DesignReference = m.DesignReference,
                DesignFileUrl = m.DesignFileUrl,
                PrintingType = m.PrintingType,
                PrintingPlacement = m.PrintingPlacement,
                IsActive = m.IsActive
            }).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<ProductFulfillmentMappingResponse> CreateMappingAsync(CreateFulfillmentMappingRequest request)
    {
        var mapping = new ProductFulfillmentMapping
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            ProductVariantId = request.ProductVariantId,
            ProviderId = request.ProviderId,
            ExternalProductId = request.ExternalProductId,
            ExternalVariantId = request.ExternalVariantId,
            ExternalSku = request.ExternalSku,
            DesignReference = request.DesignReference,
            DesignFileUrl = request.DesignFileUrl,
            PrintingType = request.PrintingType,
            PrintingPlacement = request.PrintingPlacement,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ProductFulfillmentMappings.Add(mapping);
        await _context.SaveChangesAsync();

        return new ProductFulfillmentMappingResponse
        {
            Id = mapping.Id,
            ProductId = mapping.ProductId,
            ProductVariantId = mapping.ProductVariantId,
            ProviderId = mapping.ProviderId,
            ExternalProductId = mapping.ExternalProductId,
            ExternalVariantId = mapping.ExternalVariantId,
            ExternalSku = mapping.ExternalSku,
            DesignReference = mapping.DesignReference,
            DesignFileUrl = mapping.DesignFileUrl,
            PrintingType = mapping.PrintingType,
            PrintingPlacement = mapping.PrintingPlacement,
            IsActive = mapping.IsActive
        };
    }

    public async Task<ProductFulfillmentMappingResponse?> UpdateMappingAsync(Guid id, UpdateFulfillmentMappingRequest request)
    {
        var mapping = await _context.ProductFulfillmentMappings
            .Where(m => m.Id == id && m.IsActive)
            .Include(m => m.Product)
            .Include(m => m.ProductVariant)
            .Include(m => m.Provider)
            .FirstOrDefaultAsync();

        if (mapping == null) return null;

        if (request.ExternalProductId != null) mapping.ExternalProductId = request.ExternalProductId;
        if (request.ExternalVariantId != null) mapping.ExternalVariantId = request.ExternalVariantId;
        if (request.ExternalSku != null) mapping.ExternalSku = request.ExternalSku;
        if (request.DesignReference != null) mapping.DesignReference = request.DesignReference;
        if (request.DesignFileUrl != null) mapping.DesignFileUrl = request.DesignFileUrl;
        if (request.PrintingType != null) mapping.PrintingType = request.PrintingType;
        if (request.PrintingPlacement != null) mapping.PrintingPlacement = request.PrintingPlacement;
        if (request.IsActive.HasValue) mapping.IsActive = request.IsActive.Value;
        mapping.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ProductFulfillmentMappingResponse
        {
            Id = mapping.Id,
            ProductId = mapping.ProductId,
            ProductName = mapping.Product?.Name,
            ProductVariantId = mapping.ProductVariantId,
            VariantSku = mapping.ProductVariant?.Sku,
            ProviderId = mapping.ProviderId,
            ProviderName = mapping.Provider?.Name,
            ExternalProductId = mapping.ExternalProductId,
            ExternalVariantId = mapping.ExternalVariantId,
            ExternalSku = mapping.ExternalSku,
            DesignReference = mapping.DesignReference,
            DesignFileUrl = mapping.DesignFileUrl,
            PrintingType = mapping.PrintingType,
            PrintingPlacement = mapping.PrintingPlacement,
            IsActive = mapping.IsActive
        };
    }

    public async Task<bool> DeleteMappingAsync(Guid id)
    {
        var mapping = await _context.ProductFulfillmentMappings
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

        if (mapping == null) return false;

        mapping.IsActive = false;
        mapping.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<FulfillmentOrderResponse?> RetryFulfillmentAsync(Guid fulfillmentOrderId)
    {
        var fulfillmentOrder = await _context.FulfillmentOrders
            .Where(fo => fo.Id == fulfillmentOrderId && fo.IsActive)
            .Include(fo => fo.Items.Where(i => i.IsActive))
            .Include(fo => fo.Shipment)
            .Include(fo => fo.Provider)
            .Include(fo => fo.Order)
            .FirstOrDefaultAsync();

        if (fulfillmentOrder == null) return null;

        if (fulfillmentOrder.Status != FulfillmentStatus.Failed && fulfillmentOrder.Status != FulfillmentStatus.Cancelled)
            return MapToFulfillmentOrderResponse(fulfillmentOrder);

        fulfillmentOrder.Status = FulfillmentStatus.Pending;
        fulfillmentOrder.FailureReason = null;
        fulfillmentOrder.ErrorCategory = null;
        fulfillmentOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await EnqueueSubmissionAsync(fulfillmentOrder.OrderId);

        return MapToFulfillmentOrderResponse(fulfillmentOrder);
    }

    private static FulfillmentOrderResponse MapToFulfillmentOrderResponse(FulfillmentOrder fo)
    {
        return new FulfillmentOrderResponse
        {
            Id = fo.Id,
            OrderId = fo.OrderId,
            OrderNumber = fo.Order?.OrderNumber ?? "",
            ProviderId = fo.ProviderId,
            ProviderName = fo.Provider?.Name,
            ExternalOrderId = fo.ExternalOrderId,
            Status = fo.Status.ToString(),
            ProviderStatus = fo.ProviderStatus,
            FailureReason = fo.FailureReason,
            ErrorCategory = fo.ErrorCategory,
            SubmittedAt = fo.SubmittedAt,
            CompletedAt = fo.CompletedAt,
            CreatedAt = fo.CreatedAt,
            Items = fo.Items.Select(i => new FulfillmentOrderItemResponse
            {
                Id = i.Id,
                FulfillmentOrderId = i.FulfillmentOrderId,
                OrderItemId = i.OrderItemId,
                ExternalProductId = i.ExternalProductId,
                ExternalVariantId = i.ExternalVariantId,
                ExternalSku = i.ExternalSku,
                Quantity = i.Quantity,
                Status = i.Status,
                FailureReason = i.FailureReason,
                DesignReference = i.DesignReference,
                DesignFileUrl = i.DesignFileUrl,
                MockupUrl = i.MockupUrl
            }).ToList(),
            Shipment = fo.Shipment != null ? new ShipmentResponse
            {
                Id = fo.Shipment.Id,
                FulfillmentOrderId = fo.Shipment.FulfillmentOrderId ?? fo.Id,
                TrackingNumber = fo.Shipment.TrackingNumber,
                CourierName = fo.Shipment.CourierName,
                TrackingUrl = fo.Shipment.TrackingUrl,
                ProviderShippingStatus = fo.Shipment.ProviderShippingStatus
            } : null
        };
    }
}
