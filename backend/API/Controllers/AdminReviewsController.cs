using Application.Common.Models;
using Application.DTOs.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = "Admin,Manager")]
public class AdminReviewsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminReviewsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ReviewResponse>>>> GetReviews([FromQuery] PaginatedRequest request, [FromQuery] bool? isApproved = null)
    {
        var result = await _adminService.GetReviewsAsync(request, isApproved);
        return Ok(ApiResponse<PaginatedResponse<ReviewResponse>>.SuccessResponse(result));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> UpdateReview(Guid id, [FromBody] UpdateReviewRequest request)
    {
        var result = await _adminService.UpdateReviewAsync(id, request);
        if (result == null) return NotFound(ApiResponse<ReviewResponse>.ErrorResponse("Review not found", 404));
        return Ok(ApiResponse<ReviewResponse>.SuccessResponse(result, "Review updated"));
    }

    [HttpPost("{id:guid}/reply")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> ReplyToReview(Guid id, [FromBody] ReplyReviewRequest request)
    {
        var result = await _adminService.ReplyToReviewAsync(id, request);
        if (result == null) return NotFound(ApiResponse<ReviewResponse>.ErrorResponse("Review not found", 404));
        return Ok(ApiResponse<ReviewResponse>.SuccessResponse(result, "Reply added"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteReview(Guid id)
    {
        var result = await _adminService.DeleteReviewAsync(id);
        if (!result) return NotFound(ApiResponse<object>.ErrorResponse("Review not found", 404));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Review deleted"));
    }
}
