using Application.Common.Models;
using Application.DTOs.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/admin/fulfillment")]
[Authorize(Roles = "Admin,Manager")]
[EnableRateLimiting("global")]
public class AdminFulfillmentController : ControllerBase
{
    private readonly IFulfillmentService _fulfillmentService;

    public AdminFulfillmentController(IFulfillmentService fulfillmentService)
    {
        _fulfillmentService = fulfillmentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<FulfillmentOrderResponse>>>> GetFulfillmentOrders([FromQuery] PaginatedRequest request)
    {
        var result = await _fulfillmentService.GetFulfillmentOrdersAsync(request);
        return Ok(ApiResponse<PaginatedResponse<FulfillmentOrderResponse>>.SuccessResponse(result));
    }

    [HttpGet("providers")]
    public async Task<ActionResult<ApiResponse<List<FulfillmentProviderResponse>>>> GetProviders()
    {
        var result = await _fulfillmentService.GetProvidersAsync();
        return Ok(ApiResponse<List<FulfillmentProviderResponse>>.SuccessResponse(result));
    }

    [HttpGet("mappings")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductFulfillmentMappingResponse>>>> GetMappings([FromQuery] PaginatedRequest request)
    {
        var result = await _fulfillmentService.GetMappingsAsync(request);
        return Ok(ApiResponse<PaginatedResponse<ProductFulfillmentMappingResponse>>.SuccessResponse(result));
    }

    [HttpPost("mappings")]
    public async Task<ActionResult<ApiResponse<ProductFulfillmentMappingResponse>>> CreateMapping([FromBody] CreateFulfillmentMappingRequest request)
    {
        var result = await _fulfillmentService.CreateMappingAsync(request);
        return Ok(ApiResponse<ProductFulfillmentMappingResponse>.SuccessResponse(result, "Mapping created", 201));
    }

    [HttpPut("mappings/{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductFulfillmentMappingResponse>>> UpdateMapping(Guid id, [FromBody] UpdateFulfillmentMappingRequest request)
    {
        var result = await _fulfillmentService.UpdateMappingAsync(id, request);
        if (result == null) return NotFound(ApiResponse<ProductFulfillmentMappingResponse>.ErrorResponse("Mapping not found", 404));
        return Ok(ApiResponse<ProductFulfillmentMappingResponse>.SuccessResponse(result));
    }

    [HttpDelete("mappings/{id:guid}")]
    public async Task<ActionResult> DeleteMapping(Guid id)
    {
        var deleted = await _fulfillmentService.DeleteMappingAsync(id);
        if (!deleted) return NotFound(ApiResponse<object>.ErrorResponse("Mapping not found", 404));
        return Ok(ApiResponse<object>.SuccessResponse(new { }));
    }

    [HttpPost("{id:guid}/retry")]
    public async Task<ActionResult<ApiResponse<FulfillmentOrderResponse>>> RetryFulfillment(Guid id)
    {
        var result = await _fulfillmentService.RetryFulfillmentAsync(id);
        if (result == null) return NotFound(ApiResponse<FulfillmentOrderResponse>.ErrorResponse("Fulfillment order not found", 404));
        return Ok(ApiResponse<FulfillmentOrderResponse>.SuccessResponse(result));
    }
}
