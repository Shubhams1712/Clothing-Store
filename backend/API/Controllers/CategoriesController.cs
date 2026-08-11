using Application.Common.Models;
using Application.DTOs.Storefront;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/categories")]
[EnableRateLimiting("global")]
public class CategoriesController : ControllerBase
{
    private readonly IStorefrontService _storefrontService;

    public CategoriesController(IStorefrontService storefrontService)
    {
        _storefrontService = storefrontService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StorefrontCategoryResponse>>>> GetCategories()
    {
        var result = await _storefrontService.GetCategoriesAsync();
        return Ok(ApiResponse<List<StorefrontCategoryResponse>>.SuccessResponse(result));
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponse<StorefrontCategoryResponse>>> GetCategoryBySlug(string slug)
    {
        var result = await _storefrontService.GetCategoryBySlugAsync(slug);
        if (result == null)
            return NotFound(ApiResponse<StorefrontCategoryResponse>.ErrorResponse("Category not found", 404));
        return Ok(ApiResponse<StorefrontCategoryResponse>.SuccessResponse(result));
    }
}
