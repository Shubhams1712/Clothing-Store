using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Admin;

public class ReviewResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }
    public bool IsHidden { get; set; }
    public string? AdminReply { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateReviewRequest
{
    public bool IsApproved { get; set; }
    public bool IsHidden { get; set; }
}

public class ReplyReviewRequest
{
    [Required]
    [MaxLength(2000)]
    public string AdminReply { get; set; } = string.Empty;
}
