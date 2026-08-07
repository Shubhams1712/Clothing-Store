using Application.Common.Models;
using Application.DTOs.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin,Manager")]
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
}
