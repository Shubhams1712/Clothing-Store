using Application.Common.Models;
using Application.DTOs.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/inventory")]
[Authorize(Roles = "Admin,Manager")]
public class AdminInventoryController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminInventoryController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductResponse>>>> GetInventory([FromQuery] PaginatedRequest request)
    {
        var result = await _adminService.GetInventoryAsync(request);
        return Ok(ApiResponse<PaginatedResponse<ProductResponse>>.SuccessResponse(result));
    }

    [HttpPut("{variantId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStock(Guid variantId, [FromBody] UpdateStockRequest request)
    {
        var result = await _adminService.UpdateInventoryAsync(variantId, request.Stock);
        if (!result) return BadRequest(ApiResponse<object>.ErrorResponse("Invalid variant or stock value", 400));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Stock updated"));
    }
}

public class UpdateStockRequest
{
    public int Stock { get; set; }
}
