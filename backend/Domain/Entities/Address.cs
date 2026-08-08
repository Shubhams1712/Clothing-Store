using Domain.Common;

namespace Domain.Entities;

public class Address : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? Landmark { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = "India";
    public string PostalCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
