using Domain.Enums;

namespace Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(Guid? userId, AuditAction action, string? details = null, string? ipAddress = null, string? userAgent = null, bool isSuccess = true);
}
