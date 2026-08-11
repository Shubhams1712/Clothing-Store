using Application.Common.Models;
using Application.DTOs.Storefront;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/coupons")]
[EnableRateLimiting("global")]
public class CouponsController : ControllerBase
{
    private readonly IStorefrontService _storefrontService;

    public CouponsController(IStorefrontService storefrontService)
    {
        _storefrontService = storefrontService;
    }

    [HttpPost("apply")]
    public async Task<ActionResult<ApiResponse<CouponApplyResponse>>> ApplyCoupon([FromBody] ApplyCouponRequest request)
    {
        var result = await _storefrontService.ApplyCouponAsync(request);
        if (!result.IsValid)
            return BadRequest(ApiResponse<CouponApplyResponse>.ErrorResponse(result.Message ?? "Invalid coupon"));
        return Ok(ApiResponse<CouponApplyResponse>.SuccessResponse(result));
    }
}
