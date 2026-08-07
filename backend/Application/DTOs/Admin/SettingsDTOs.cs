using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Admin;

public class StoreSettingsResponse
{
    public Guid Id { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string? StoreDescription { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? Currency { get; set; }
    public string? CurrencySymbol { get; set; }
    public string? TaxRate { get; set; }
    public string? ShippingPolicy { get; set; }
    public string? ReturnPolicy { get; set; }
    public string? PrivacyPolicy { get; set; }
    public string? TermsOfService { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SocialFacebook { get; set; }
    public string? SocialInstagram { get; set; }
    public string? SocialTwitter { get; set; }
    public string? SocialYoutube { get; set; }
    public string? RazorpayKeyId { get; set; }
    public string? RazorpayKeySecret { get; set; }
    public string? CloudinaryCloudName { get; set; }
    public string? CloudinaryApiKey { get; set; }
    public string? CloudinaryApiSecret { get; set; }
}

public class UpdateStoreSettingsRequest
{
    [Required]
    [MaxLength(200)]
    public string StoreName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? StoreDescription { get; set; }

    [MaxLength(256)]
    [EmailAddress]
    public string? ContactEmail { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(10)]
    public string? Currency { get; set; }

    [MaxLength(10)]
    public string? CurrencySymbol { get; set; }

    [MaxLength(20)]
    public string? TaxRate { get; set; }

    [MaxLength(2000)]
    public string? ShippingPolicy { get; set; }

    [MaxLength(2000)]
    public string? ReturnPolicy { get; set; }

    [MaxLength(5000)]
    public string? PrivacyPolicy { get; set; }

    [MaxLength(5000)]
    public string? TermsOfService { get; set; }

    [MaxLength(1000)]
    public string? LogoUrl { get; set; }

    [MaxLength(1000)]
    public string? FaviconUrl { get; set; }

    [MaxLength(20)]
    public string? PrimaryColor { get; set; }

    [MaxLength(500)]
    public string? SocialFacebook { get; set; }

    [MaxLength(500)]
    public string? SocialInstagram { get; set; }

    [MaxLength(500)]
    public string? SocialTwitter { get; set; }

    [MaxLength(500)]
    public string? SocialYoutube { get; set; }

    [MaxLength(200)]
    public string? RazorpayKeyId { get; set; }

    [MaxLength(500)]
    public string? RazorpayKeySecret { get; set; }

    [MaxLength(200)]
    public string? CloudinaryCloudName { get; set; }

    [MaxLength(200)]
    public string? CloudinaryApiKey { get; set; }

    [MaxLength(500)]
    public string? CloudinaryApiSecret { get; set; }
}
