using Application.Common.Models;
using Application.DTOs.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin,Manager")]
[EnableRateLimiting("global")]
public class AdminCategoriesController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminCategoriesController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CategoryResponse>>>> GetCategories([FromQuery] PaginatedRequest request)
    {
        var result = await _adminService.GetCategoriesAsync(request);
        return Ok(ApiResponse<PaginatedResponse<CategoryResponse>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> GetCategory(Guid id)
    {
        var result = await _adminService.GetCategoryByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<CategoryResponse>.ErrorResponse("Category not found", 404));
        return Ok(ApiResponse<CategoryResponse>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var result = await _adminService.CreateCategoryAsync(request);
        return CreatedAtAction(nameof(GetCategory), new { id = result.Id }, ApiResponse<CategoryResponse>.SuccessResponse(result, "Category created", 201));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _adminService.UpdateCategoryAsync(id, request);
        if (result == null) return NotFound(ApiResponse<CategoryResponse>.ErrorResponse("Category not found", 404));
        return Ok(ApiResponse<CategoryResponse>.SuccessResponse(result, "Category updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCategory(Guid id)
    {
        var result = await _adminService.DeleteCategoryAsync(id);
        if (!result) return NotFound(ApiResponse<object>.ErrorResponse("Category not found or has products", 404));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Category deleted"));
    }
}
