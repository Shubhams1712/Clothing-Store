using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class VerifyEmailRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
