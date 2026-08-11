using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<HealthCheck> HealthChecks => Set<HealthCheck>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRoleEntity> UserRoles => Set<UserRoleEntity>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionProduct> CollectionProducts => Set<CollectionProduct>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<StoreSettings> StoreSettings => Set<StoreSettings>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<FulfillmentProvider> FulfillmentProviders => Set<FulfillmentProvider>();
    public DbSet<ProductFulfillmentMapping> ProductFulfillmentMappings => Set<ProductFulfillmentMapping>();
    public DbSet<FulfillmentOrder> FulfillmentOrders => Set<FulfillmentOrder>();
    public DbSet<FulfillmentOrderItem> FulfillmentOrderItems => Set<FulfillmentOrderItem>();
    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HealthCheck>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Service).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Message).HasMaxLength(500);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<UserRoleEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User).WithMany(u => u.UserRoles).HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Role).WithMany(r => r.UserRoles).HasForeignKey(e => e.RoleId);
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.RefreshTokens).HasForeignKey(e => e.UserId);
            entity.HasIndex(e => e.Token).IsUnique();
        });

        modelBuilder.Entity<EmailVerificationToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.EmailVerificationTokens).HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.PasswordResetTokens).HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Details).HasMaxLength(1000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ShortDescription).HasMaxLength(500);
            entity.Property(e => e.Sku).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ComparePrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CostPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.SeoTitle).HasMaxLength(200);
            entity.Property(e => e.SeoDescription).HasMaxLength(500);
            entity.Property(e => e.QikinkProductId).HasMaxLength(100);
            entity.Property(e => e.QikinkProductName).HasMaxLength(200);
            entity.Property(e => e.DesignReference).HasMaxLength(200);
            entity.Property(e => e.DesignFileUrl).HasMaxLength(1000);
            entity.Property(e => e.MockupUrl).HasMaxLength(1000);
            entity.HasIndex(e => e.Slug).IsUnique().HasFilter("\"IsActive\" = true");
            entity.HasIndex(e => e.Sku).IsUnique().HasFilter("\"IsActive\" = true");
            entity.HasIndex(e => new { e.IsActive, e.IsPublished, e.CreatedAt });
            entity.HasIndex(e => new { e.IsActive, e.CategoryId });
            entity.HasIndex(e => new { e.IsActive, e.Price });
            entity.HasOne(e => e.Category).WithMany(c => c.Products).HasForeignKey(e => e.CategoryId);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Size).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Sku).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.QikinkSku).HasMaxLength(100);
            entity.HasIndex(e => e.Sku).IsUnique().HasFilter("\"IsActive\" = true");
            entity.HasIndex(e => new { e.ProductId, e.IsActive, e.Stock });
            entity.HasOne(e => e.Product).WithMany(p => p.Variants).HasForeignKey(e => e.ProductId);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.CloudinaryPublicId).HasMaxLength(500);
            entity.Property(e => e.AltText).HasMaxLength(200);
            entity.HasIndex(e => new { e.ProductId, e.IsActive });
            entity.HasOne(e => e.Product).WithMany(p => p.Images).HasForeignKey(e => e.ProductId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.Parent).WithMany(c => c.Children).HasForeignKey(e => e.ParentId);
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<CollectionProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Collection).WithMany(c => c.CollectionProducts).HasForeignKey(e => e.CollectionId);
            entity.HasOne(e => e.Product).WithMany(p => p.CollectionProducts).HasForeignKey(e => e.ProductId);
            entity.HasIndex(e => new { e.CollectionId, e.ProductId }).IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ShippingAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.ShippingName).HasMaxLength(200);
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);
            entity.Property(e => e.ShippingCity).HasMaxLength(100);
            entity.Property(e => e.ShippingState).HasMaxLength(100);
            entity.Property(e => e.ShippingPostalCode).HasMaxLength(20);
            entity.Property(e => e.ShippingCountry).HasMaxLength(100);
            entity.Property(e => e.ShippingPhone).HasMaxLength(20);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);
            entity.Property(e => e.PaymentId).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.InternalNotes).HasMaxLength(1000);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.PaymentId).HasFilter("\"PaymentId\" IS NOT NULL AND \"PaymentMethod\" = 'Razorpay'").IsUnique();
            entity.HasIndex(e => new { e.IsActive, e.Status });
            entity.HasIndex(e => new { e.IsActive, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.IsActive, e.CreatedAt });
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Sku).HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.Property(e => e.Size).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => new { e.OrderId, e.IsActive });
            entity.HasOne(e => e.Order).WithMany(o => o.Items).HasForeignKey(e => e.OrderId);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
            entity.HasOne(e => e.ProductVariant).WithMany().HasForeignKey(e => e.ProductVariantId);
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Value).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MinimumOrderAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MaximumDiscountAmount).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Comment).HasMaxLength(2000);
            entity.Property(e => e.AdminReply).HasMaxLength(2000);
            entity.HasIndex(e => new { e.ProductId, e.IsActive, e.IsApproved });
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.AddressLine1).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AddressLine2).HasMaxLength(500);
            entity.Property(e => e.Landmark).HasMaxLength(200);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => new { e.UserId, e.IsActive });
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<StoreSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StoreName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.StoreDescription).HasMaxLength(2000);
            entity.Property(e => e.ContactEmail).HasMaxLength(256);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.CurrencySymbol).HasMaxLength(10);
            entity.Property(e => e.TaxRate).HasMaxLength(20);
            entity.Property(e => e.ShippingPolicy).HasMaxLength(2000);
            entity.Property(e => e.ReturnPolicy).HasMaxLength(2000);
            entity.Property(e => e.PrivacyPolicy).HasMaxLength(5000);
            entity.Property(e => e.TermsOfService).HasMaxLength(5000);
            entity.Property(e => e.LogoUrl).HasMaxLength(1000);
            entity.Property(e => e.FaviconUrl).HasMaxLength(1000);
            entity.Property(e => e.PrimaryColor).HasMaxLength(20);
            entity.Property(e => e.SocialFacebook).HasMaxLength(500);
            entity.Property(e => e.SocialInstagram).HasMaxLength(500);
            entity.Property(e => e.SocialTwitter).HasMaxLength(500);
            entity.Property(e => e.SocialYoutube).HasMaxLength(500);
            entity.Property(e => e.RazorpayKeyId).HasMaxLength(200);
            entity.Property(e => e.RazorpayKeySecret).HasMaxLength(500);
            entity.Property(e => e.RazorpayWebhookSecret).HasMaxLength(500);
            entity.Property(e => e.CloudinaryCloudName).HasMaxLength(200);
            entity.Property(e => e.CloudinaryApiKey).HasMaxLength(200);
            entity.Property(e => e.CloudinaryApiSecret).HasMaxLength(500);
        });

        modelBuilder.Entity<FulfillmentProvider>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.ApiBaseUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<ProductFulfillmentMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExternalProductId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExternalVariantId).HasMaxLength(100);
            entity.Property(e => e.ExternalSku).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DesignReference).HasMaxLength(200);
            entity.Property(e => e.DesignFileUrl).HasMaxLength(1000);
            entity.Property(e => e.PrintingType).HasMaxLength(50);
            entity.Property(e => e.PrintingPlacement).HasMaxLength(50);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
            entity.HasOne(e => e.ProductVariant).WithMany().HasForeignKey(e => e.ProductVariantId);
            entity.HasOne(e => e.Provider).WithMany(p => p.ProductMappings).HasForeignKey(e => e.ProviderId);
            entity.HasIndex(e => new { e.ProductId, e.ProductVariantId, e.ProviderId }).IsUnique();
        });

        modelBuilder.Entity<FulfillmentOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExternalOrderId).HasMaxLength(100);
            entity.Property(e => e.ProviderStatus).HasMaxLength(100);
            entity.Property(e => e.FailureReason).HasMaxLength(2000);
            entity.Property(e => e.ErrorCategory).HasMaxLength(100);
            entity.HasIndex(e => e.Status);
            entity.HasOne(e => e.Order).WithOne(o => o.FulfillmentOrder).HasForeignKey<FulfillmentOrder>(e => e.OrderId);
            entity.HasOne(e => e.Provider).WithMany(p => p.FulfillmentOrders).HasForeignKey(e => e.ProviderId);
            entity.HasIndex(e => e.ExternalOrderId);
        });

        modelBuilder.Entity<FulfillmentOrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExternalProductId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExternalVariantId).HasMaxLength(100);
            entity.Property(e => e.ExternalSku).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(100);
            entity.Property(e => e.FailureReason).HasMaxLength(1000);
            entity.Property(e => e.DesignReference).HasMaxLength(200);
            entity.Property(e => e.DesignFileUrl).HasMaxLength(1000);
            entity.Property(e => e.MockupUrl).HasMaxLength(1000);
            entity.HasOne(e => e.FulfillmentOrder).WithMany(fo => fo.Items).HasForeignKey(e => e.FulfillmentOrderId);
            entity.HasOne(e => e.OrderItem).WithMany().HasForeignKey(e => e.OrderItemId);
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TrackingNumber).HasMaxLength(100);
            entity.Property(e => e.CourierName).HasMaxLength(100);
            entity.Property(e => e.TrackingUrl).HasMaxLength(1000);
            entity.Property(e => e.ProviderShippingStatus).HasMaxLength(100);
            entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId);
            entity.HasOne(e => e.FulfillmentOrder).WithOne(fo => fo.Shipment).HasForeignKey<Shipment>(e => e.FulfillmentOrderId);
        });

        SeedRoles(modelBuilder);
    }

    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Customer", Description = "Customer role", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true },
            new Role { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Admin", Description = "Administrator role", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true },
            new Role { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Manager", Description = "Manager role", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true },
            new Role { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Staff", Description = "Staff role", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true }
        );
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
