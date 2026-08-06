using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public AuditAction Action { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsSuccess { get; set; }
}
