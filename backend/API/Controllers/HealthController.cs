using Application.Common.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthService _healthService;

    public HealthController(HealthService healthService)
    {
        _healthService = healthService;
    }

    [HttpGet]
    public ActionResult<ApiResponse<HealthCheckResponse>> GetHealth()
    {
        var result = _healthService.GetHealth();
        return Ok(result);
    }
}
