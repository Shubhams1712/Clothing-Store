using Application.Common.Models;
using Application.DTOs.Common;
using Application.DTOs.Storefront;

namespace Application.Interfaces;

public interface IStorefrontService
{
    Task<PaginatedResponse<StorefrontProductResponse>> GetProductsAsync(ProductFilterRequest request);
    Task<StorefrontProductResponse?> GetProductBySlugAsync(string slug);
    Task<PaginatedResponse<StorefrontProductResponse>> GetFeaturedProductsAsync(int page = 1, int pageSize = 8);
    Task<PaginatedResponse<StorefrontProductResponse>> GetNewArrivalsAsync(int page = 1, int pageSize = 20);
    Task<PaginatedResponse<StorefrontProductResponse>> GetBestSellersAsync(int page = 1, int pageSize = 20);
    Task<List<string>> GetAvailableSizesAsync();
    Task<List<string>> GetAvailableColorsAsync();
    Task<List<StorefrontCategoryResponse>> GetCategoriesAsync();
    Task<List<StorefrontCollectionResponse>> GetCollectionsAsync();
    Task<List<StorefrontCollectionResponse>> GetFeaturedCollectionsAsync();
    Task<StorefrontCollectionResponse?> GetCollectionBySlugAsync(string slug);
    Task<StorefrontCategoryResponse?> GetCategoryBySlugAsync(string slug);
    Task<PaginatedResponse<StorefrontReviewResponse>> GetProductReviewsAsync(Guid productId, int page = 1, int pageSize = 10, string? sortBy = null);
    Task<StorefrontRatingDistribution> GetProductRatingDistributionAsync(Guid productId);
    Task<StorefrontReviewResponse?> CreateProductReviewAsync(Guid productId, Guid userId, CreateStorefrontReviewRequest request);
    Task<List<AddressResponse>> GetAddressesAsync(Guid userId);
    Task<AddressResponse?> CreateAddressAsync(Guid userId, CreateAddressRequest request);
    Task<AddressResponse?> UpdateAddressAsync(Guid userId, Guid addressId, CreateAddressRequest request);
    Task<bool> DeleteAddressAsync(Guid userId, Guid addressId);
    Task<CouponApplyResponse> ApplyCouponAsync(ApplyCouponRequest request);
    Task<CheckoutReviewResponse> ReviewCheckoutAsync(Guid userId, CheckoutReviewRequest request);
}
