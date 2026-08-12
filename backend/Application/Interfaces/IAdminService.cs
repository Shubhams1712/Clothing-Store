using Application.DTOs.Admin;
using Application.DTOs.Common;

namespace Application.Interfaces;

public interface IAdminService
{
    Task<DashboardStatsResponse> GetDashboardStatsAsync();
    Task<PaginatedResponse<ProductResponse>> GetProductsAsync(PaginatedRequest request);
    Task<ProductResponse?> GetProductByIdAsync(Guid id);
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request);
    Task<ProductResponse?> UpdateProductAsync(Guid id, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(Guid id);
    Task<bool> ToggleProductPublishAsync(Guid id);
    Task<PaginatedResponse<CategoryResponse>> GetCategoriesAsync(PaginatedRequest request);
    Task<CategoryResponse?> GetCategoryByIdAsync(Guid id);
    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
    Task<CategoryResponse?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(Guid id);
    Task<PaginatedResponse<CollectionResponse>> GetCollectionsAsync(PaginatedRequest request);
    Task<CollectionResponse?> GetCollectionByIdAsync(Guid id);
    Task<CollectionResponse> CreateCollectionAsync(CreateCollectionRequest request);
    Task<CollectionResponse?> UpdateCollectionAsync(Guid id, UpdateCollectionRequest request);
    Task<bool> DeleteCollectionAsync(Guid id);
    Task<PaginatedResponse<OrderResponse>> GetOrdersAsync(PaginatedRequest request);
    Task<OrderResponse?> GetOrderByIdAsync(Guid id);
    Task<OrderResponse?> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusRequest request);
    Task<PaginatedResponse<CustomerResponse>> GetCustomersAsync(PaginatedRequest request);
    Task<CustomerDetailResponse?> GetCustomerByIdAsync(Guid id);
    Task<bool> ToggleCustomerActiveAsync(Guid id);
    Task<bool> SetUserAdminStatusAsync(Guid userId, bool isAdmin);
    Task<PaginatedResponse<CouponResponse>> GetCouponsAsync(PaginatedRequest request);
    Task<CouponResponse?> GetCouponByIdAsync(Guid id);
    Task<CouponResponse> CreateCouponAsync(CreateCouponRequest request);
    Task<CouponResponse?> UpdateCouponAsync(Guid id, UpdateCouponRequest request);
    Task<bool> DeleteCouponAsync(Guid id);
    Task<PaginatedResponse<ReviewResponse>> GetReviewsAsync(PaginatedRequest request, bool? isApproved = null);
    Task<ReviewResponse?> UpdateReviewAsync(Guid id, UpdateReviewRequest request);
    Task<ReviewResponse?> ReplyToReviewAsync(Guid id, ReplyReviewRequest request);
    Task<bool> DeleteReviewAsync(Guid id);
    Task<StoreSettingsResponse?> GetSettingsAsync();
    Task<StoreSettingsResponse> UpdateSettingsAsync(UpdateStoreSettingsRequest request);
    Task<PaginatedResponse<ProductResponse>> GetInventoryAsync(PaginatedRequest request);
    Task<bool> UpdateInventoryAsync(Guid variantId, int stock);
}
