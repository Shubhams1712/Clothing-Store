using Domain.Common;

namespace Domain.Entities;

public class HealthCheck : BaseEntity
{
    public string Service { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
}
