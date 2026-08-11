using Application.Common.Models;
using Application.DTOs.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/admin/collections")]
[Authorize(Roles = "Admin,Manager")]
[EnableRateLimiting("global")]
public class AdminCollectionsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminCollectionsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CollectionResponse>>>> GetCollections([FromQuery] PaginatedRequest request)
    {
        var result = await _adminService.GetCollectionsAsync(request);
        return Ok(ApiResponse<PaginatedResponse<CollectionResponse>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CollectionResponse>>> GetCollection(Guid id)
    {
        var result = await _adminService.GetCollectionByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<CollectionResponse>.ErrorResponse("Collection not found", 404));
        return Ok(ApiResponse<CollectionResponse>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CollectionResponse>>> CreateCollection([FromBody] CreateCollectionRequest request)
    {
        var result = await _adminService.CreateCollectionAsync(request);
        return CreatedAtAction(nameof(GetCollection), new { id = result.Id }, ApiResponse<CollectionResponse>.SuccessResponse(result, "Collection created", 201));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CollectionResponse>>> UpdateCollection(Guid id, [FromBody] UpdateCollectionRequest request)
    {
        var result = await _adminService.UpdateCollectionAsync(id, request);
        if (result == null) return NotFound(ApiResponse<CollectionResponse>.ErrorResponse("Collection not found", 404));
        return Ok(ApiResponse<CollectionResponse>.SuccessResponse(result, "Collection updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCollection(Guid id)
    {
        var result = await _adminService.DeleteCollectionAsync(id);
        if (!result) return NotFound(ApiResponse<object>.ErrorResponse("Collection not found", 404));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Collection deleted"));
    }
}
