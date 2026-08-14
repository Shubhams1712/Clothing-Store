using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class DevEmailService : IEmailService
{
    private readonly ILogger<DevEmailService> _logger;
    private readonly EmailSettings _settings;

    public DevEmailService(ILogger<DevEmailService> logger, IOptions<EmailSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public Task<bool> SendVerificationEmailAsync(string toEmail, string token, string email)
    {
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(email);
        var frontendUrl = string.IsNullOrEmpty(_settings.FrontendUrl) ? "http://localhost:3000" : _settings.FrontendUrl;
        var verificationUrl = $"{frontendUrl}/verify-email?token={encodedToken}&email={encodedEmail}";

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
        var frontendUrl = string.IsNullOrEmpty(_settings.FrontendUrl) ? "http://localhost:3000" : _settings.FrontendUrl;
        var resetUrl = $"{frontendUrl}/reset-password?token={encodedToken}&email={encodedEmail}";

        _logger.LogInformation("=== PASSWORD RESET (DevEmailService) ===");
        _logger.LogInformation("To: {Email}", toEmail);
        _logger.LogInformation("Reset Link: {Link}", resetUrl);
        _logger.LogInformation("=========================================");

        return Task.FromResult(true);
    }
}
