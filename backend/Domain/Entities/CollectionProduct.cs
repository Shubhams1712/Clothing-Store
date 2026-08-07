using Domain.Common;

namespace Domain.Entities;

public class CollectionProduct : BaseEntity
{
    public Guid CollectionId { get; set; }
    public Collection Collection { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int SortOrder { get; set; }
}
