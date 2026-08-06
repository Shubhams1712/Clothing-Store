namespace Domain.Common;

public abstract class BaseEntity<TId> where TId : notnull
{
    public TId Id { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
