using Application.Common.Models;
using Application.DTOs.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin,Manager")]
[EnableRateLimiting("global")]
public class AdminProductsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminProductsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductResponse>>>> GetProducts([FromQuery] PaginatedRequest request)
    {
        var result = await _adminService.GetProductsAsync(request);
        return Ok(ApiResponse<PaginatedResponse<ProductResponse>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> GetProduct(Guid id)
    {
        var result = await _adminService.GetProductByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<ProductResponse>.ErrorResponse("Product not found", 404));
        return Ok(ApiResponse<ProductResponse>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> CreateProduct([FromBody] CreateProductRequest request)
    {
        var result = await _adminService.CreateProductAsync(request);
        return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, ApiResponse<ProductResponse>.SuccessResponse(result, "Product created", 201));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var result = await _adminService.UpdateProductAsync(id, request);
        if (result == null) return NotFound(ApiResponse<ProductResponse>.ErrorResponse("Product not found", 404));
        return Ok(ApiResponse<ProductResponse>.SuccessResponse(result, "Product updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(Guid id)
    {
        var result = await _adminService.DeleteProductAsync(id);
        if (!result) return NotFound(ApiResponse<object>.ErrorResponse("Product not found", 404));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Product deleted"));
    }

    [HttpPatch("{id:guid}/toggle-publish")]
    public async Task<ActionResult<ApiResponse<object>>> TogglePublish(Guid id)
    {
        var result = await _adminService.ToggleProductPublishAsync(id);
        if (!result) return NotFound(ApiResponse<object>.ErrorResponse("Product not found", 404));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Product status toggled"));
    }

    [HttpPost("bulk-import")]
    public async Task<ActionResult<ApiResponse<BulkProductImportResponse>>> BulkImport([FromBody] BulkProductImportRequest request)
    {
        if (request.Products == null || request.Products.Count == 0)
            return BadRequest(ApiResponse<BulkProductImportResponse>.ErrorResponse("No products provided", 400));

        if (request.Products.Count > 500)
            return BadRequest(ApiResponse<BulkProductImportResponse>.ErrorResponse("Maximum 500 products per import", 400));

        var result = await _adminService.BulkImportProductsAsync(request);
        return Ok(ApiResponse<BulkProductImportResponse>.SuccessResponse(result, $"Import complete: {result.SuccessCount} succeeded, {result.FailureCount} failed"));
    }

    [HttpPost("bulk-delete")]
    public async Task<ActionResult<ApiResponse<object>>> BulkDelete([FromBody] BulkDeleteRequest request)
    {
        if (request.Ids == null || request.Ids.Count == 0)
            return BadRequest(ApiResponse<object>.ErrorResponse("No product IDs provided", 400));

        if (request.Ids.Count > 100)
            return BadRequest(ApiResponse<object>.ErrorResponse("Maximum 100 products per bulk delete", 400));

        var deleted = await _adminService.BulkDeleteProductsAsync(request.Ids);
        return Ok(ApiResponse<object>.SuccessResponse(new { deletedCount = deleted }, $"{deleted} products deleted"));
    }
}

public class BulkDeleteRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public List<Guid> Ids { get; set; } = new();
}
