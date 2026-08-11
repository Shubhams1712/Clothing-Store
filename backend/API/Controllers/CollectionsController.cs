using Application.Common.Models;
using Application.DTOs.Storefront;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/collections")]
[EnableRateLimiting("global")]
public class CollectionsController : ControllerBase
{
    private readonly IStorefrontService _storefrontService;

    public CollectionsController(IStorefrontService storefrontService)
    {
        _storefrontService = storefrontService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StorefrontCollectionResponse>>>> GetCollections()
    {
        var result = await _storefrontService.GetCollectionsAsync();
        return Ok(ApiResponse<List<StorefrontCollectionResponse>>.SuccessResponse(result));
    }

    [HttpGet("featured")]
    public async Task<ActionResult<ApiResponse<List<StorefrontCollectionResponse>>>> GetFeaturedCollections()
    {
        var result = await _storefrontService.GetFeaturedCollectionsAsync();
        return Ok(ApiResponse<List<StorefrontCollectionResponse>>.SuccessResponse(result));
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponse<StorefrontCollectionResponse>>> GetCollectionBySlug(string slug)
    {
        var result = await _storefrontService.GetCollectionBySlugAsync(slug);
        if (result == null)
            return NotFound(ApiResponse<StorefrontCollectionResponse>.ErrorResponse("Collection not found", 404));
        return Ok(ApiResponse<StorefrontCollectionResponse>.SuccessResponse(result));
    }
}
