using System.Security.Claims;
using Application.Common.Models;
using Application.DTOs.Storefront;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/checkout")]
[Authorize]
[EnableRateLimiting("global")]
public class CheckoutController : ControllerBase
{
    private readonly IStorefrontService _storefrontService;

    public CheckoutController(IStorefrontService storefrontService)
    {
        _storefrontService = storefrontService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    [HttpPost("review")]
    public async Task<ActionResult<ApiResponse<CheckoutReviewResponse>>> ReviewCheckout([FromBody] CheckoutReviewRequest request)
    {
        var result = await _storefrontService.ReviewCheckoutAsync(GetCurrentUserId(), request);
        if (!result.IsValid && result.Errors.Count > 0)
            return BadRequest(ApiResponse<CheckoutReviewResponse>.ErrorResponse(string.Join("; ", result.Errors)));
        return Ok(ApiResponse<CheckoutReviewResponse>.SuccessResponse(result));
    }
}
