using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

namespace Infrastructure.Services;

public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly EmailSettings _settings;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(
        IResend resend,
        IOptions<EmailSettings> settings,
        ILogger<ResendEmailService> logger)
    {
        _resend = resend;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> SendVerificationEmailAsync(string toEmail, string token, string email)
    {
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(email);
        var verificationUrl = $"{_settings.FrontendUrl.TrimEnd('/')}/verify-email?token={encodedToken}&email={encodedEmail}";

        var subject = "Verify your email address";
        var htmlBody = BuildVerificationEmailHtml(verificationUrl);
        var textBody = BuildVerificationEmailText(verificationUrl);

        return await SendEmailAsync(toEmail, subject, htmlBody, textBody, "verification");
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string token, string email)
    {
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(email);
        var resetUrl = $"{_settings.FrontendUrl.TrimEnd('/')}/reset-password?token={encodedToken}&email={encodedEmail}";

        var subject = "Reset your password";
        var htmlBody = BuildPasswordResetEmailHtml(resetUrl);
        var textBody = BuildPasswordResetEmailText(resetUrl);

        return await SendEmailAsync(toEmail, subject, htmlBody, textBody, "password reset");
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string textBody, string emailType)
    {
        try
        {
            _logger.LogInformation("Sending {EmailType} email to {Email}", emailType, MaskEmail(toEmail));

            var message = new EmailMessage
            {
                From = $"{_settings.FromName} <{_settings.FromEmail}>",
                Subject = subject,
                HtmlBody = htmlBody,
                TextBody = textBody
            };
            message.To.Add(toEmail);

            var result = await _resend.EmailSendAsync(message);

            if (result is null)
            {
                _logger.LogWarning("Resend returned null for {EmailType} email to {Email}", emailType, MaskEmail(toEmail));
                return false;
            }

            _logger.LogInformation("Successfully sent {EmailType} email to {Email}, MessageId: {MessageId}",
                emailType, MaskEmail(toEmail), result);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send {EmailType} email to {Email}", emailType, MaskEmail(toEmail));
            return false;
        }
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return "***";

        var parts = email.Split('@');
        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
            return $"***@{domain}";

        return $"{localPart[0]}***{localPart[^1]}@{domain}";
    }

    private static string BuildVerificationEmailHtml(string verificationUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Verify Your Email</title>
</head>
<body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: #f8f9fa; border-radius: 8px; padding: 30px; text-align: center;'>
        <h1 style='color: #1a1a1a; margin-bottom: 20px;'>Verify Your Email Address</h1>
        <p style='color: #555; margin-bottom: 25px;'>Thank you for registering! Please click the button below to verify your email address.</p>
        <a href=""{verificationUrl}"" style=""display: inline-block; background-color: #1a1a1a; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: 500; margin-bottom: 20px;"">Verify Email Address</a>
        <p style='color: #777; font-size: 14px; margin-top: 20px;'>This link will expire in 24 hours.</p>
        <p style='color: #777; font-size: 14px;'>If you did not create an account, please ignore this email.</p>
    </div>
    <p style='color: #999; font-size: 12px; text-align: center; margin-top: 20px;'>If the button above does not work, copy and paste this link into your browser:<br><a href=""{verificationUrl}"">{verificationUrl}</a></p>
</body>
</html>";
    }

    private static string BuildVerificationEmailText(string verificationUrl)
    {
        return $@"
Verify Your Email Address

Thank you for registering! Please visit the following link to verify your email address:

{verificationUrl}

This link will expire in 24 hours.

If you did not create an account, please ignore this email.";
    }

    private static string BuildPasswordResetEmailHtml(string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Password</title>
</head>
<body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: #f8f9fa; border-radius: 8px; padding: 30px; text-align: center;'>
        <h1 style='color: #1a1a1a; margin-bottom: 20px;'>Reset Your Password</h1>
        <p style='color: #555; margin-bottom: 25px;'>You requested a password reset. Please click the button below to reset your password.</p>
        <a href=""{resetUrl}"" style=""display: inline-block; background-color: #1a1a1a; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: 500; margin-bottom: 20px;"">Reset Password</a>
        <p style='color: #777; font-size: 14px; margin-top: 20px;'>This link will expire in 1 hour.</p>
        <p style='color: #777; font-size: 14px;'>If you did not request a password reset, please ignore this email.</p>
    </div>
    <p style='color: #999; font-size: 12px; text-align: center; margin-top: 20px;'>If the button above does not work, copy and paste this link into your browser:<br><a href=""{resetUrl}"">{resetUrl}</a></p>
</body>
</html>";
    }

    private static string BuildPasswordResetEmailText(string resetUrl)
    {
        return $@"
Reset Your Password

You requested a password reset. Please visit the following link to reset your password:

{resetUrl}

This link will expire in 1 hour.

If you did not request a password reset, please ignore this email.";
    }
}
