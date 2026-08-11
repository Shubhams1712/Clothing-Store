using System.Security.Claims;
using Application.Common.Models;
using Application.DTOs.Common;
using Application.DTOs.Storefront;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/products")]
[EnableRateLimiting("global")]
public class ProductsController : ControllerBase
{
    private readonly IStorefrontService _storefrontService;

    public ProductsController(IStorefrontService storefrontService)
    {
        _storefrontService = storefrontService;
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? Guid.Parse(claim.Value) : null;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<StorefrontProductResponse>>>> GetProducts([FromQuery] ProductFilterRequest request)
    {
        var result = await _storefrontService.GetProductsAsync(request);
        return Ok(ApiResponse<PaginatedResponse<StorefrontProductResponse>>.SuccessResponse(result));
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponse<StorefrontProductResponse>>> GetProductBySlug(string slug)
    {
        var result = await _storefrontService.GetProductBySlugAsync(slug);
        if (result == null)
            return NotFound(ApiResponse<StorefrontProductResponse>.ErrorResponse("Product not found", 404));
        return Ok(ApiResponse<StorefrontProductResponse>.SuccessResponse(result));
    }

    [HttpGet("featured")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<StorefrontProductResponse>>>> GetFeaturedProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 8)
    {
        var result = await _storefrontService.GetFeaturedProductsAsync(page, pageSize);
        return Ok(ApiResponse<PaginatedResponse<StorefrontProductResponse>>.SuccessResponse(result));
    }

    [HttpGet("new-arrivals")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<StorefrontProductResponse>>>> GetNewArrivals([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _storefrontService.GetNewArrivalsAsync(page, pageSize);
        return Ok(ApiResponse<PaginatedResponse<StorefrontProductResponse>>.SuccessResponse(result));
    }

    [HttpGet("best-sellers")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<StorefrontProductResponse>>>> GetBestSellers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _storefrontService.GetBestSellersAsync(page, pageSize);
        return Ok(ApiResponse<PaginatedResponse<StorefrontProductResponse>>.SuccessResponse(result));
    }

    [HttpGet("sizes")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetAvailableSizes()
    {
        var result = await _storefrontService.GetAvailableSizesAsync();
        return Ok(ApiResponse<List<string>>.SuccessResponse(result));
    }

    [HttpGet("colors")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetAvailableColors()
    {
        var result = await _storefrontService.GetAvailableColorsAsync();
        return Ok(ApiResponse<List<string>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}/reviews")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<StorefrontReviewResponse>>>> GetProductReviews(
        Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = null)
    {
        var result = await _storefrontService.GetProductReviewsAsync(id, page, pageSize, sortBy);
        return Ok(ApiResponse<PaginatedResponse<StorefrontReviewResponse>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}/rating-distribution")]
    public async Task<ActionResult<ApiResponse<StorefrontRatingDistribution>>> GetRatingDistribution(Guid id)
    {
        var result = await _storefrontService.GetProductRatingDistributionAsync(id);
        return Ok(ApiResponse<StorefrontRatingDistribution>.SuccessResponse(result));
    }

    [Authorize]
    [HttpPost("{id:guid}/reviews")]
    public async Task<ActionResult<ApiResponse<StorefrontReviewResponse>>> CreateReview(
        Guid id, [FromBody] CreateStorefrontReviewRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<StorefrontReviewResponse>.ErrorResponse("Unauthorized", 401));

        var result = await _storefrontService.CreateProductReviewAsync(id, userId.Value, request);
        if (result == null)
            return BadRequest(ApiResponse<StorefrontReviewResponse>.ErrorResponse("Unable to create review. Product not found or you have already reviewed this product."));

        return Ok(ApiResponse<StorefrontReviewResponse>.SuccessResponse(result, "Review submitted for approval"));
    }
}
