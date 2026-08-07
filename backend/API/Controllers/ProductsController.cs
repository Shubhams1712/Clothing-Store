using Application.Common.Models;
using Application.DTOs.Common;
using Application.DTOs.Storefront;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IStorefrontService _storefrontService;

    public ProductsController(IStorefrontService storefrontService)
    {
        _storefrontService = storefrontService;
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
}
