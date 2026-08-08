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

public class AddressResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? Landmark { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = "India";
    public string PostalCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class CreateAddressRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(256)]
    public string? Email { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string AddressLine1 { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? AddressLine2 { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string? Landmark { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string State { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string Country { get; set; } = "India";

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}

public class ApplyCouponRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    public decimal OrderSubtotal { get; set; }
}

public class CouponApplyResponse
{
    public bool IsValid { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Message { get; set; }
}

public class CheckoutReviewRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public List<CheckoutItemRequest> Items { get; set; } = new();
    public string? CouponCode { get; set; }
    public string? ShippingCountry { get; set; }
    public string? ShippingState { get; set; }
}

public class CheckoutItemRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public Guid ProductId { get; set; }

    public Guid? VariantId { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public class CheckoutReviewResponse
{
    public List<CheckoutItemResponse> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public CouponApplyResponse? Coupon { get; set; }
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class CheckoutItemResponse
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public int AvailableStock { get; set; }
    public bool IsAvailable { get; set; }
}
