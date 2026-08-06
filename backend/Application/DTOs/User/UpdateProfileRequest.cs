using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User;

public class UpdateProfileRequest
{
    [MinLength(2)]
    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MinLength(2)]
    [MaxLength(50)]
    public string? LastName { get; set; }
}
