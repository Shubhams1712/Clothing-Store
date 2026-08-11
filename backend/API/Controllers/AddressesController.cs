using System.Security.Claims;
using Application.Common.Models;
using Application.DTOs.Storefront;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/addresses")]
[Authorize]
[EnableRateLimiting("global")]
public class AddressesController : ControllerBase
{
    private readonly IStorefrontService _storefrontService;

    public AddressesController(IStorefrontService storefrontService)
    {
        _storefrontService = storefrontService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AddressResponse>>>> GetAddresses()
    {
        var result = await _storefrontService.GetAddressesAsync(GetCurrentUserId());
        return Ok(ApiResponse<List<AddressResponse>>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AddressResponse>>> CreateAddress([FromBody] CreateAddressRequest request)
    {
        var result = await _storefrontService.CreateAddressAsync(GetCurrentUserId(), request);
        if (result == null)
            return BadRequest(ApiResponse<AddressResponse>.ErrorResponse("Failed to create address"));
        return Ok(ApiResponse<AddressResponse>.SuccessResponse(result, "Address created", 201));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AddressResponse>>> UpdateAddress(Guid id, [FromBody] CreateAddressRequest request)
    {
        var result = await _storefrontService.UpdateAddressAsync(GetCurrentUserId(), id, request);
        if (result == null)
            return NotFound(ApiResponse<AddressResponse>.ErrorResponse("Address not found", 404));
        return Ok(ApiResponse<AddressResponse>.SuccessResponse(result, "Address updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAddress(Guid id)
    {
        var result = await _storefrontService.DeleteAddressAsync(GetCurrentUserId(), id);
        if (!result)
            return NotFound(ApiResponse<object>.ErrorResponse("Address not found", 404));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Address deleted"));
    }
}
