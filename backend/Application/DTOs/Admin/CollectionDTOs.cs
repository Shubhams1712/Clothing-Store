using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Admin;

public class CreateCollectionRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? ImageUrl { get; set; }

    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public List<Guid> ProductIds { get; set; } = new();
}

public class UpdateCollectionRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? ImageUrl { get; set; }

    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public List<Guid> ProductIds { get; set; } = new();
}

public class CollectionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductBriefResponse> Products { get; set; } = new();
}

public class ProductBriefResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
}
