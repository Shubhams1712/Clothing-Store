using Application.DTOs.Common;
using Application.DTOs.Storefront;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class StorefrontService : IStorefrontService
{
    private readonly ApplicationDbContext _context;

    public StorefrontService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponse<StorefrontProductResponse>> GetProductsAsync(ProductFilterRequest request)
    {
        var pageSize = Math.Min(request.PageSize, 50);
        var query = _context.Products
            .Where(p => p.IsActive && p.IsPublished)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants.Where(v => v.IsAvailable))
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) ||
                (p.Description != null && p.Description.ToLower().Contains(search)) ||
                (p.Brand != null && p.Brand.ToLower().Contains(search)));
        }

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId);
        else if (!string.IsNullOrWhiteSpace(request.CategorySlug))
            query = query.Where(p => p.Category != null && p.Category.Slug == request.CategorySlug && p.Category.IsActive);

        if (request.CollectionId.HasValue)
            query = query.Where(p => p.CollectionProducts.Any(cp => cp.CollectionId == request.CollectionId && cp.Collection.IsActive));
        else if (!string.IsNullOrWhiteSpace(request.CollectionSlug))
            query = query.Where(p => p.CollectionProducts.Any(cp => cp.Collection.Slug == request.CollectionSlug && cp.Collection.IsActive));

        if (!string.IsNullOrWhiteSpace(request.Size))
            query = query.Where(p => p.Variants.Any(v => v.Size == request.Size && v.IsAvailable));

        if (!string.IsNullOrWhiteSpace(request.Color))
            query = query.Where(p => p.Variants.Any(v => v.Color == request.Color && v.IsAvailable));

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice);

        if (request.IsFeatured == true)
            query = query.Where(p => p.IsFeatured);

        if (request.IsNewArrival == true)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            query = query.Where(p => p.CreatedAt >= thirtyDaysAgo);
        }

        if (request.InStock == true)
            query = query.Where(p => p.Variants.Any(v => v.IsAvailable && v.Stock > 0));

        var totalCount = await query.CountAsync();

        query = request.SortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "best_sellers" => query.OrderByDescending(p => p.Variants.Sum(v => v.Stock)),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var products = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PaginatedResponse<StorefrontProductResponse>
        {
            Items = products.Select(p => MapToStorefrontProduct(p)).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<StorefrontProductResponse?> GetProductBySlugAsync(string slug)
    {
        var product = await _context.Products
            .Where(p => p.Slug == slug && p.IsActive && p.IsPublished)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (product == null) return null;

        var reviewStats = await _context.Reviews
            .Where(r => r.ProductId == product.Id && r.IsApproved)
            .GroupBy(r => 1)
            .Select(g => new { Count = g.Count(), Avg = g.Average(r => r.Rating) })
            .FirstOrDefaultAsync();

        var response = MapToStorefrontProduct(product);
        response.ReviewCount = reviewStats?.Count ?? 0;
        response.AverageRating = reviewStats != null ? (decimal)reviewStats.Avg : 0;
        return response;
    }

    public async Task<PaginatedResponse<StorefrontProductResponse>> GetFeaturedProductsAsync(int page = 1, int pageSize = 8)
    {
        return await GetProductsAsync(new ProductFilterRequest
        {
            Page = page,
            PageSize = pageSize,
            IsFeatured = true,
            SortBy = "newest"
        });
    }

    public async Task<PaginatedResponse<StorefrontProductResponse>> GetNewArrivalsAsync(int page = 1, int pageSize = 20)
    {
        return await GetProductsAsync(new ProductFilterRequest
        {
            Page = page,
            PageSize = pageSize,
            IsNewArrival = true,
            SortBy = "newest"
        });
    }

    public async Task<PaginatedResponse<StorefrontProductResponse>> GetBestSellersAsync(int page = 1, int pageSize = 20)
    {
        return await GetProductsAsync(new ProductFilterRequest
        {
            Page = page,
            PageSize = pageSize,
            SortBy = "best_sellers"
        });
    }

    public async Task<List<string>> GetAvailableSizesAsync()
    {
        return await _context.ProductVariants
            .Where(v => v.IsAvailable && v.Stock > 0 && v.Size != null)
            .Select(v => v.Size!)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }

    public async Task<List<string>> GetAvailableColorsAsync()
    {
        return await _context.ProductVariants
            .Where(v => v.IsAvailable && v.Stock > 0 && v.Color != null)
            .Select(v => v.Color!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<List<StorefrontCategoryResponse>> GetCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .Select(c => new StorefrontCategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ProductCount = c.Products.Count(p => p.IsActive && p.IsPublished)
            })
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<StorefrontCollectionResponse>> GetCollectionsAsync()
    {
        return await _context.Collections
            .Where(c => c.IsActive)
            .Select(c => new StorefrontCollectionResponse
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                IsFeatured = c.IsFeatured,
                ProductCount = c.CollectionProducts.Count(cp => cp.Product.IsActive && cp.Product.IsPublished)
            })
            .OrderByDescending(c => c.IsFeatured)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<StorefrontCollectionResponse>> GetFeaturedCollectionsAsync()
    {
        return await _context.Collections
            .Where(c => c.IsActive && c.IsFeatured)
            .Select(c => new StorefrontCollectionResponse
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                IsFeatured = c.IsFeatured,
                ProductCount = c.CollectionProducts.Count(cp => cp.Product.IsActive && cp.Product.IsPublished)
            })
            .ToListAsync();
    }

    public async Task<StorefrontCollectionResponse?> GetCollectionBySlugAsync(string slug)
    {
        return await _context.Collections
            .Where(c => c.Slug == slug && c.IsActive)
            .Select(c => new StorefrontCollectionResponse
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                IsFeatured = c.IsFeatured,
                ProductCount = c.CollectionProducts.Count(cp => cp.Product.IsActive && cp.Product.IsPublished)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<StorefrontCategoryResponse?> GetCategoryBySlugAsync(string slug)
    {
        return await _context.Categories
            .Where(c => c.Slug == slug && c.IsActive)
            .Select(c => new StorefrontCategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ProductCount = c.Products.Count(p => p.IsActive && p.IsPublished)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PaginatedResponse<StorefrontReviewResponse>> GetProductReviewsAsync(Guid productId, int page = 1, int pageSize = 10, string? sortBy = null)
    {
        var query = _context.Reviews
            .Where(r => r.ProductId == productId && r.IsActive && r.IsApproved && !r.IsHidden)
            .Include(r => r.User)
            .AsQueryable();

        query = sortBy?.ToLower() switch
        {
            "rating_high" => query.OrderByDescending(r => r.Rating),
            "rating_low" => query.OrderBy(r => r.Rating),
            "oldest" => query.OrderBy(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var reviews = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new StorefrontReviewResponse
            {
                Id = r.Id,
                UserName = r.User.FirstName + " " + r.User.LastName,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                AdminReply = r.AdminReply,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<StorefrontReviewResponse>
        {
            Items = reviews,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<StorefrontRatingDistribution> GetProductRatingDistributionAsync(Guid productId)
    {
        var ratings = await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsActive && r.IsApproved && !r.IsHidden)
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync();

        var distribution = new StorefrontRatingDistribution();
        foreach (var item in ratings)
        {
            switch (item.Rating)
            {
                case 5: distribution.FiveStar = item.Count; break;
                case 4: distribution.FourStar = item.Count; break;
                case 3: distribution.ThreeStar = item.Count; break;
                case 2: distribution.TwoStar = item.Count; break;
                case 1: distribution.OneStar = item.Count; break;
            }
        }
        return distribution;
    }

    public async Task<StorefrontReviewResponse?> CreateProductReviewAsync(Guid productId, Guid userId, CreateStorefrontReviewRequest request)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null || !product.IsActive) return null;

        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId && r.IsActive);
        if (existingReview != null) return null;

        var review = new Domain.Entities.Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = request.Rating,
            Title = request.Title,
            Comment = request.Comment,
            IsApproved = false,
            IsHidden = false
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        return new StorefrontReviewResponse
        {
            Id = review.Id,
            UserName = user?.FirstName + " " + user?.LastName,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }

    public async Task<List<AddressResponse>> GetAddressesAsync(Guid userId)
    {
        return await _context.Addresses
            .Where(a => a.UserId == userId && a.IsActive)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new AddressResponse
            {
                Id = a.Id,
                FullName = a.FullName,
                Phone = a.Phone,
                Email = a.Email,
                AddressLine1 = a.AddressLine1,
                AddressLine2 = a.AddressLine2,
                Landmark = a.Landmark,
                City = a.City,
                State = a.State,
                Country = a.Country,
                PostalCode = a.PostalCode,
                IsDefault = a.IsDefault
            })
            .ToListAsync();
    }

    public async Task<AddressResponse?> CreateAddressAsync(Guid userId, CreateAddressRequest request)
    {
        if (request.IsDefault)
        {
            var existing = await _context.Addresses
                .Where(a => a.UserId == userId && a.IsActive && a.IsDefault)
                .ToListAsync();
            foreach (var addr in existing)
                addr.IsDefault = false;
        }

        var address = new Domain.Entities.Address
        {
            UserId = userId,
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            Landmark = request.Landmark,
            City = request.City,
            State = request.State,
            Country = request.Country,
            PostalCode = request.PostalCode,
            IsDefault = request.IsDefault
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        return new AddressResponse
        {
            Id = address.Id,
            FullName = address.FullName,
            Phone = address.Phone,
            Email = address.Email,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            Landmark = address.Landmark,
            City = address.City,
            State = address.State,
            Country = address.Country,
            PostalCode = address.PostalCode,
            IsDefault = address.IsDefault
        };
    }

    public async Task<AddressResponse?> UpdateAddressAsync(Guid userId, Guid addressId, CreateAddressRequest request)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && a.IsActive);
        if (address == null) return null;

        if (request.IsDefault && !address.IsDefault)
        {
            var existing = await _context.Addresses
                .Where(a => a.UserId == userId && a.IsActive && a.IsDefault && a.Id != addressId)
                .ToListAsync();
            foreach (var addr in existing)
                addr.IsDefault = false;
        }

        address.FullName = request.FullName;
        address.Phone = request.Phone;
        address.Email = request.Email;
        address.AddressLine1 = request.AddressLine1;
        address.AddressLine2 = request.AddressLine2;
        address.Landmark = request.Landmark;
        address.City = request.City;
        address.State = request.State;
        address.Country = request.Country;
        address.PostalCode = request.PostalCode;
        address.IsDefault = request.IsDefault;

        await _context.SaveChangesAsync();

        return new AddressResponse
        {
            Id = address.Id,
            FullName = address.FullName,
            Phone = address.Phone,
            Email = address.Email,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            Landmark = address.Landmark,
            City = address.City,
            State = address.State,
            Country = address.Country,
            PostalCode = address.PostalCode,
            IsDefault = address.IsDefault
        };
    }

    public async Task<bool> DeleteAddressAsync(Guid userId, Guid addressId)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && a.IsActive);
        if (address == null) return false;

        address.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CouponApplyResponse> ApplyCouponAsync(ApplyCouponRequest request)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == request.Code && c.IsActive);

        if (coupon == null)
            return new CouponApplyResponse { IsValid = false, Message = "Invalid coupon code" };

        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < DateTime.UtcNow)
            return new CouponApplyResponse { IsValid = false, Message = "Coupon has expired" };

        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit)
            return new CouponApplyResponse { IsValid = false, Message = "Coupon usage limit reached" };

        if (coupon.MinimumOrderAmount.HasValue && request.OrderSubtotal < coupon.MinimumOrderAmount)
            return new CouponApplyResponse
            {
                IsValid = false,
                Message = $"Minimum order amount is {coupon.MinimumOrderAmount.Value}"
            };

        decimal discount = coupon.Type == Domain.Enums.CouponType.Percentage
            ? request.OrderSubtotal * coupon.Value / 100
            : coupon.Value;

        if (coupon.MaximumDiscountAmount.HasValue && discount > coupon.MaximumDiscountAmount)
            discount = coupon.MaximumDiscountAmount.Value;

        discount = Math.Round(discount, 2);

        return new CouponApplyResponse
        {
            IsValid = true,
            Code = coupon.Code,
            Description = coupon.Description,
            Type = coupon.Type.ToString(),
            Value = coupon.Value,
            DiscountAmount = discount,
            Message = "Coupon applied successfully"
        };
    }

    public async Task<CheckoutReviewResponse> ReviewCheckoutAsync(Guid userId, CheckoutReviewRequest request)
    {
        var response = new CheckoutReviewResponse { IsValid = true };

        if (request.Items == null || request.Items.Count == 0)
        {
            response.IsValid = false;
            response.Errors.Add("Cart cannot be empty");
            return response;
        }

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive && p.IsPublished)
            .Include(p => p.Variants.Where(v => v.IsActive))
            .Include(p => p.Images.Where(i => i.IsActive))
            .ToListAsync();

        foreach (var item in request.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null)
            {
                response.IsValid = false;
                response.Errors.Add($"Product not found: {item.ProductId}");
                continue;
            }

            var variant = item.VariantId.HasValue
                ? product.Variants.FirstOrDefault(v => v.Id == item.VariantId.Value)
                : product.Variants.FirstOrDefault();

            var unitPrice = variant?.Price ?? product.Price;
            var stock = variant?.Stock ?? 0;
            var isAvailable = variant?.IsAvailable ?? false;
            var imageUrl = product.Images.FirstOrDefault(i => i.IsFeatured)?.Url
                ?? product.Images.FirstOrDefault()?.Url;

            if (!isAvailable || stock < item.Quantity)
            {
                response.IsValid = false;
                response.Errors.Add($"{product.Name} is insufficient stock (available: {stock})");
            }

            response.Items.Add(new CheckoutItemResponse
            {
                ProductId = product.Id,
                VariantId = variant?.Id,
                ProductName = product.Name,
                Sku = variant?.Sku ?? product.Sku,
                ImageUrl = imageUrl,
                UnitPrice = unitPrice,
                Quantity = item.Quantity,
                TotalPrice = unitPrice * item.Quantity,
                AvailableStock = stock,
                IsAvailable = isAvailable && stock >= item.Quantity
            });
        }

        response.SubTotal = response.Items.Sum(i => i.TotalPrice);
        response.TaxAmount = 0;
        response.ShippingAmount = response.SubTotal >= 2000 ? 0 : 150;

        if (!string.IsNullOrEmpty(request.CouponCode))
        {
            var couponResult = await ApplyCouponAsync(new ApplyCouponRequest
            {
                Code = request.CouponCode,
                OrderSubtotal = response.SubTotal
            });
            if (couponResult.IsValid)
            {
                response.Coupon = couponResult;
                response.DiscountAmount = couponResult.DiscountAmount;
            }
        }

        response.TotalAmount = response.SubTotal + response.TaxAmount + response.ShippingAmount - response.DiscountAmount;
        if (response.TotalAmount < 0) response.TotalAmount = 0;

        return response;
    }

    private static StorefrontProductResponse MapToStorefrontProduct(Domain.Entities.Product product)
    {
        var images = product.Images.Where(i => i.IsActive).OrderBy(i => i.SortOrder).ToList();
        var variants = product.Variants.Where(v => v.IsActive).ToList();
        var primaryImage = images.FirstOrDefault(i => i.IsFeatured) ?? images.FirstOrDefault();
        var secondaryImage = images.FirstOrDefault(i => !i.IsFeatured && i != primaryImage);

        return new StorefrontProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            ShortDescription = product.ShortDescription,
            Sku = product.Sku,
            Price = product.Price,
            ComparePrice = product.ComparePrice,
            Brand = product.Brand,
            IsFeatured = product.IsFeatured,
            CategoryName = product.Category?.Name,
            CategorySlug = product.Category?.Slug,
            CreatedAt = product.CreatedAt,
            PrimaryImageUrl = primaryImage?.Url,
            SecondaryImageUrl = secondaryImage?.Url,
            Images = images.Select(i => new StorefrontProductImageResponse
            {
                Id = i.Id,
                Url = i.Url,
                AltText = i.AltText,
                SortOrder = i.SortOrder,
                IsFeatured = i.IsFeatured
            }).ToList(),
            Colors = variants.Select(v => v.Color).Where(c => c != null).Distinct().Cast<string>().ToList(),
            Sizes = variants.Select(v => v.Size).Where(s => s != null).Distinct().Cast<string>().ToList(),
            Variants = variants.Select(v => new StorefrontProductVariantResponse
            {
                Id = v.Id,
                Size = v.Size,
                Color = v.Color,
                Sku = v.Sku,
                Price = v.Price,
                Stock = v.Stock,
                IsAvailable = v.IsAvailable
            }).ToList(),
            IsInStock = variants.Any(v => v.IsAvailable && v.Stock > 0)
        };
    }
}
