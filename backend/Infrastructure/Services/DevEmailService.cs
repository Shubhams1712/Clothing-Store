using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class DevEmailService : IEmailService
{
    private readonly ILogger<DevEmailService> _logger;

    public DevEmailService(ILogger<DevEmailService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendVerificationEmailAsync(string toEmail, string token, string email)
    {
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(email);
        var verificationUrl = $"http://localhost:3000/verify-email?token={encodedToken}&email={encodedEmail}";

        _logger.LogInformation("=== EMAIL VERIFICATION (DevEmailService) ===");
        _logger.LogInformation("To: {Email}", toEmail);
        _logger.LogInformation("Verification Link: {Link}", verificationUrl);
        _logger.LogInformation("============================================");

        return Task.FromResult(true);
    }

    public Task<bool> SendPasswordResetEmailAsync(string toEmail, string token, string email)
    {
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(email);
        var resetUrl = $"http://localhost:3000/reset-password?token={encodedToken}&email={encodedEmail}";

        _logger.LogInformation("=== PASSWORD RESET (DevEmailService) ===");
        _logger.LogInformation("To: {Email}", toEmail);
        _logger.LogInformation("Reset Link: {Link}", resetUrl);
        _logger.LogInformation("=========================================");

        return Task.FromResult(true);
    }
}
