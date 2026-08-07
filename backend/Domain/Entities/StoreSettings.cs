using Domain.Common;

namespace Domain.Entities;

public class StoreSettings : BaseEntity
{
    public string StoreName { get; set; } = string.Empty;
    public string? StoreDescription { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? Currency { get; set; } = "INR";
    public string? CurrencySymbol { get; set; } = "₹";
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
