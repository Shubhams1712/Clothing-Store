using System.Security.Claims;
using Application.Common.Models;
using Application.DTOs.Admin;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin,Manager,Staff")]
[EnableRateLimiting("global")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(IAdminService adminService, ApplicationDbContext context, ILogger<AdminDashboardController> logger)
    {
        _adminService = adminService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardStatsResponse>>> GetDashboardStats()
    {
        // Diagnostic logging
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var roleClaims = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var isAdminClaim = User.FindFirst("is_admin")?.Value;
        var allClaims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();

        _logger.LogWarning("[ADMIN DASHBOARD] User: {UserId} ({Email}), RoleClaims: {Roles}, IsAdminClaim: {IsAdmin}, AllClaimTypes: {ClaimTypes}",
            userId, email, string.Join(",", roleClaims), isAdminClaim, string.Join(",", allClaims.Select(c => c.Type.Split('/').Last())));

        // Check database IsAdmin value
        if (Guid.TryParse(userId, out var parsedUserId))
        {
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == parsedUserId);
            if (dbUser != null)
            {
                var dbRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == parsedUserId && ur.IsActive)
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .ToListAsync();
                _logger.LogWarning("[ADMIN DASHBOARD] DB IsAdmin: {DbIsAdmin}, DB Roles: {DbRoles}",
                    dbUser.IsAdmin, string.Join(",", dbRoles));
            }
        }

        var result = await _adminService.GetDashboardStatsAsync();
        return Ok(ApiResponse<DashboardStatsResponse>.SuccessResponse(result));
    }
}
