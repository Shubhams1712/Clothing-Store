using Domain.Common;

namespace Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string? Brand { get; set; }
    public string? Tags { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }

    public bool IsQikinkProduct { get; set; }
    public string? QikinkProductId { get; set; }
    public string? QikinkProductName { get; set; }
    public string? DesignReference { get; set; }
    public string? DesignFileUrl { get; set; }
    public string? MockupUrl { get; set; }

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<CollectionProduct> CollectionProducts { get; set; } = new List<CollectionProduct>();
}
