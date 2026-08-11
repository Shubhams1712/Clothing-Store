using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Admin;

public class CreateProductRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    [Required]
    [MaxLength(100)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    public decimal? ComparePrice { get; set; }
    public decimal? CostPrice { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
    public Guid? CategoryId { get; set; }

    [MaxLength(200)]
    public string? SeoTitle { get; set; }

    [MaxLength(500)]
    public string? SeoDescription { get; set; }

    public bool IsQikinkProduct { get; set; }

    [MaxLength(100)]
    public string? QikinkProductId { get; set; }

    [MaxLength(200)]
    public string? QikinkProductName { get; set; }

    [MaxLength(200)]
    public string? DesignReference { get; set; }

    [MaxLength(1000)]
    public string? DesignFileUrl { get; set; }

    [MaxLength(1000)]
    public string? MockupUrl { get; set; }

    public List<CreateProductVariantRequest> Variants { get; set; } = new();
    public List<CreateProductImageRequest> Images { get; set; } = new();
}

public class UpdateProductRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    [Required]
    [MaxLength(100)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    public decimal? ComparePrice { get; set; }
    public decimal? CostPrice { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
    public Guid? CategoryId { get; set; }

    [MaxLength(200)]
    public string? SeoTitle { get; set; }

    [MaxLength(500)]
    public string? SeoDescription { get; set; }

    public bool IsQikinkProduct { get; set; }

    [MaxLength(100)]
    public string? QikinkProductId { get; set; }

    [MaxLength(200)]
    public string? QikinkProductName { get; set; }

    [MaxLength(200)]
    public string? DesignReference { get; set; }

    [MaxLength(1000)]
    public string? DesignFileUrl { get; set; }

    [MaxLength(1000)]
    public string? MockupUrl { get; set; }

    public List<CreateProductVariantRequest> Variants { get; set; } = new();
    public List<CreateProductImageRequest> Images { get; set; } = new();
}

public class CreateProductVariantRequest
{
    [MaxLength(50)]
    public string? Size { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    [Required]
    [MaxLength(100)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public bool IsAvailable { get; set; } = true;

    [MaxLength(100)]
    public string? QikinkSku { get; set; }
}

public class CreateProductImageRequest
{
    [Required]
    [MaxLength(1000)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? AltText { get; set; }

    public int SortOrder { get; set; }
    public bool IsFeatured { get; set; }
}

public class ProductResponse
{
    public Guid Id { get; set; }
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
    public bool IsActive { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public bool IsQikinkProduct { get; set; }
    public string? QikinkProductId { get; set; }
    public string? QikinkProductName { get; set; }
    public string? DesignReference { get; set; }
    public string? DesignFileUrl { get; set; }
    public string? MockupUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProductVariantResponse> Variants { get; set; } = new();
    public List<ProductImageResponse> Images { get; set; } = new();
}

public class ProductVariantResponse
{
    public Guid Id { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsAvailable { get; set; }
    public string? QikinkSku { get; set; }
}

public class ProductImageResponse
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsFeatured { get; set; }
}
