namespace Application.DTOs.Storefront;

public class StorefrontProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public string? Brand { get; set; }
    public bool IsFeatured { get; set; }
    public string? CategoryName { get; set; }
    public string? CategorySlug { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public string? SecondaryImageUrl { get; set; }
    public List<StorefrontProductImageResponse> Images { get; set; } = new();
    public List<string> Colors { get; set; } = new();
    public List<string> Sizes { get; set; } = new();
    public List<StorefrontProductVariantResponse> Variants { get; set; } = new();
    public int ReviewCount { get; set; }
    public decimal AverageRating { get; set; }
    public bool IsInStock { get; set; }
}

public class StorefrontProductImageResponse
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsFeatured { get; set; }
}

public class StorefrontProductVariantResponse
{
    public Guid Id { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsAvailable { get; set; }
}

public class StorefrontCategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int ProductCount { get; set; }
}

public class StorefrontCollectionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public int ProductCount { get; set; }
}

public class StorefrontReviewResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public string? AdminReply { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StorefrontRatingDistribution
{
    public int FiveStar { get; set; }
    public int FourStar { get; set; }
    public int ThreeStar { get; set; }
    public int TwoStar { get; set; }
    public int OneStar { get; set; }
}

public class CreateStorefrontReviewRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.Range(1, 5)]
    public int Rating { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(2000)]
    public string? Comment { get; set; }
}

public class ProductFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategorySlug { get; set; }
    public Guid? CollectionId { get; set; }
    public string? CollectionSlug { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsNewArrival { get; set; }
    public bool? IsBestSeller { get; set; }
    public bool? InStock { get; set; }
}
