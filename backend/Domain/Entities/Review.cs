using Domain.Common;

namespace Domain.Entities;

public class Review : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }
    public bool IsHidden { get; set; }
    public string? AdminReply { get; set; }
}
