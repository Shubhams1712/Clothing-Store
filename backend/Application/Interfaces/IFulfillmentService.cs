using Application.DTOs.Admin;
using Application.DTOs.Common;

namespace Application.Interfaces;

public interface IFulfillmentService
{
    Task EnqueueSubmissionAsync(Guid orderId);
    Task<object?> SubmitOrderAsync(Guid orderId);
    Task<object?> GetOrderStatusAsync(Guid orderId);

    Task<FulfillmentOrderResponse?> GetFulfillmentOrderAsync(Guid orderId);
    Task<PaginatedResponse<FulfillmentOrderResponse>> GetFulfillmentOrdersAsync(PaginatedRequest request);
    Task<List<FulfillmentProviderResponse>> GetProvidersAsync();
    Task<PaginatedResponse<ProductFulfillmentMappingResponse>> GetMappingsAsync(PaginatedRequest request);
    Task<ProductFulfillmentMappingResponse> CreateMappingAsync(CreateFulfillmentMappingRequest request);
    Task<ProductFulfillmentMappingResponse?> UpdateMappingAsync(Guid id, UpdateFulfillmentMappingRequest request);
    Task<bool> DeleteMappingAsync(Guid id);
    Task<FulfillmentOrderResponse?> RetryFulfillmentAsync(Guid fulfillmentOrderId);
}
