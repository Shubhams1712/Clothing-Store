namespace Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendVerificationEmailAsync(string toEmail, string token, string email);
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string token, string email);
}

public class EmailSendResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
