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
}
