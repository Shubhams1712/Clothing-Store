using Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class HealthService
{
    private readonly ILogger<HealthService> _logger;

    public HealthService(ILogger<HealthService> logger)
    {
        _logger = logger;
    }

    public ApiResponse<HealthCheckResponse> GetHealth()
    {
        _logger.LogInformation("Health check requested");

        var response = new HealthCheckResponse
        {
            Status = "Healthy",
            Service = "API",
            Timestamp = DateTime.UtcNow
        };

        return ApiResponse<HealthCheckResponse>.SuccessResponse(response);
    }
}
