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
        var query = _context.Products
            .Where(p => p.IsActive && p.IsPublished)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants.Where(v => v.IsAvailable))
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
            .FirstOrDefaultAsync();

        if (product == null) return null;

        var reviews = await _context.Reviews
            .Where(r => r.ProductId == product.Id && r.IsApproved)
            .ToListAsync();

        var response = MapToStorefrontProduct(product);
        response.ReviewCount = reviews.Count;
        response.AverageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;
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
