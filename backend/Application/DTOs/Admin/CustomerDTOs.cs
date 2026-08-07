namespace Application.DTOs.Admin;

public class CustomerResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class CustomerDetailResponse : CustomerResponse
{
    public List<OrderResponse> RecentOrders { get; set; } = new();
    public string? Notes { get; set; }
}
