using Domain.Common;

namespace Domain.Entities;

public class Collection : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }

    public ICollection<CollectionProduct> CollectionProducts { get; set; } = new List<CollectionProduct>();
}
