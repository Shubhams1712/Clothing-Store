using Application.Common.Models;
using Application.DTOs.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/admin/coupons")]
[Authorize(Roles = "Admin,Manager")]
[EnableRateLimiting("global")]
public class AdminCouponsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminCouponsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CouponResponse>>>> GetCoupons([FromQuery] PaginatedRequest request)
    {
        var result = await _adminService.GetCouponsAsync(request);
        return Ok(ApiResponse<PaginatedResponse<CouponResponse>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CouponResponse>>> GetCoupon(Guid id)
    {
        var result = await _adminService.GetCouponByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<CouponResponse>.ErrorResponse("Coupon not found", 404));
        return Ok(ApiResponse<CouponResponse>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CouponResponse>>> CreateCoupon([FromBody] CreateCouponRequest request)
    {
        var result = await _adminService.CreateCouponAsync(request);
        return CreatedAtAction(nameof(GetCoupon), new { id = result.Id }, ApiResponse<CouponResponse>.SuccessResponse(result, "Coupon created", 201));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CouponResponse>>> UpdateCoupon(Guid id, [FromBody] UpdateCouponRequest request)
    {
        var result = await _adminService.UpdateCouponAsync(id, request);
        if (result == null) return NotFound(ApiResponse<CouponResponse>.ErrorResponse("Coupon not found", 404));
        return Ok(ApiResponse<CouponResponse>.SuccessResponse(result, "Coupon updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCoupon(Guid id)
    {
        var result = await _adminService.DeleteCouponAsync(id);
        if (!result) return NotFound(ApiResponse<object>.ErrorResponse("Coupon not found", 404));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Coupon deleted"));
    }
}
