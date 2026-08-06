using Application.Common.Models;
using Application.DTOs.User;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("users")]
    public ActionResult<ApiResponse<object>> GetUsers()
    {
        return Ok(ApiResponse<object>.SuccessResponse(new { message = "Admin users endpoint" }));
    }
}
