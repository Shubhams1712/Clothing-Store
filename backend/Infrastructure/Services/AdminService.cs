using Application.DTOs.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;

    public AdminService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsResponse> GetDashboardStatsAsync()
    {
        var totalRevenue = await _context.Orders
            .Where(o => o.IsActive && o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded)
            .SumAsync(o => o.TotalAmount);

        var today = DateTime.UtcNow.Date;
        var todayOrders = await _context.Orders
            .CountAsync(o => o.IsActive && o.CreatedAt >= today);

        var pendingOrders = await _context.Orders
            .CountAsync(o => o.IsActive && o.Status == OrderStatus.PendingPayment);

        var lowStockProducts = await _context.ProductVariants
            .CountAsync(v => v.IsActive && v.Stock < 10);

        var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
        var totalCustomers = await _context.Users.CountAsync(u => u.IsActive && !u.IsAdmin);
        var totalOrders = await _context.Orders.CountAsync(o => o.IsActive);

        var recentOrders = await _context.Orders
            .Join(_context.Users, o => o.UserId, u => u.Id, (o, u) => new { o, u })
            .Where(x => x.o.IsActive)
            .OrderByDescending(x => x.o.CreatedAt)
            .Take(5)
            .Select(x => new OrderResponse
            {
                Id = x.o.Id,
                OrderNumber = x.o.OrderNumber,
                UserId = x.o.UserId,
                CustomerName = x.u.FirstName + " " + x.u.LastName,
                CustomerEmail = x.u.Email,
                Status = x.o.Status,
                TotalAmount = x.o.TotalAmount,
                CreatedAt = x.o.CreatedAt
            })
            .ToListAsync();

        var validOrderIds = await _context.Orders
            .Where(o => o.IsActive && o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded)
            .Select(o => o.Id)
            .ToListAsync();

        var topSellingProducts = await _context.OrderItems
            .Where(oi => oi.IsActive && validOrderIds.Contains(oi.OrderId))
            .GroupBy(oi => new { oi.ProductId, oi.ProductName })
            .Select(g => new { ProductId = g.Key.ProductId, ProductName = g.Key.ProductName, TotalSold = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.TotalSold)
            .Take(5)
            .ToListAsync();

        var topProductIds = topSellingProducts.Select(x => x.ProductId).ToList();
        var topProducts = await _context.Products
            .Where(p => topProductIds.Contains(p.Id))
            .ToListAsync();

        var topProductImageUrls = await _context.ProductImages
            .Where(pi => topProductIds.Contains(pi.ProductId) && pi.IsFeatured)
            .GroupBy(pi => pi.ProductId)
            .Select(g => new { ProductId = g.Key, Url = g.OrderBy(x => x.SortOrder).First().Url })
            .ToListAsync();

        var topSelling = topSellingProducts.Select(tp =>
        {
            var product = topProducts.FirstOrDefault(p => p.Id == tp.ProductId);
            var imageUrl = topProductImageUrls.FirstOrDefault(i => i.ProductId == tp.ProductId)?.Url;
            return new ProductBriefResponse
            {
                Id = tp.ProductId,
                Name = tp.ProductName,
                Slug = product?.Slug ?? "",
                Price = product?.Price ?? 0,
                ImageUrl = imageUrl
            };
        }).ToList();

        var latestCustomers = await _context.Users
            .Where(u => u.IsActive && !u.IsAdmin)
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new CustomerResponse
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsEmailVerified = u.IsEmailVerified,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        var salesOverviewRaw = await _context.Orders
            .Where(o => o.IsActive && o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded && o.CreatedAt >= today.AddDays(-30))
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Revenue = g.Sum(o => o.TotalAmount), Orders = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var salesOverview = salesOverviewRaw.Select(x => new SalesOverviewPoint
        {
            Date = x.Date.ToString("MMM dd"),
            Revenue = x.Revenue,
            Orders = x.Orders
        }).ToList();

        return new DashboardStatsResponse
        {
            TotalRevenue = totalRevenue,
            TodayOrders = todayOrders,
            PendingOrders = pendingOrders,
            LowStockProducts = lowStockProducts,
            TotalProducts = totalProducts,
            TotalCustomers = totalCustomers,
            TotalOrders = totalOrders,
            RecentOrders = recentOrders,
            TopSellingProducts = topSelling,
            LatestCustomers = latestCustomers,
            SalesOverview = salesOverview
        };
    }

    public async Task<PaginatedResponse<ProductResponse>> GetProductsAsync(PaginatedRequest request)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) || p.Sku.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => request.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "createdat" => request.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Description = p.Description,
                ShortDescription = p.ShortDescription,
                Sku = p.Sku,
                Price = p.Price,
                ComparePrice = p.ComparePrice,
                CostPrice = p.CostPrice,
                Brand = p.Brand,
                Tags = p.Tags,
                IsFeatured = p.IsFeatured,
                IsPublished = p.IsPublished,
                IsActive = p.IsActive,
                SeoTitle = p.SeoTitle,
                SeoDescription = p.SeoDescription,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<ProductResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<ProductResponse?> GetProductByIdAsync(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants.Where(v => v.IsActive))
            .Include(p => p.Images.Where(i => i.IsActive).OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null) return null;

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            ShortDescription = product.ShortDescription,
            Sku = product.Sku,
            Price = product.Price,
            ComparePrice = product.ComparePrice,
            CostPrice = product.CostPrice,
            Brand = product.Brand,
            Tags = product.Tags,
            IsFeatured = product.IsFeatured,
            IsPublished = product.IsPublished,
            IsActive = product.IsActive,
            SeoTitle = product.SeoTitle,
            SeoDescription = product.SeoDescription,
            IsQikinkProduct = product.IsQikinkProduct,
            QikinkProductId = product.QikinkProductId,
            QikinkProductName = product.QikinkProductName,
            DesignReference = product.DesignReference,
            DesignFileUrl = product.DesignFileUrl,
            MockupUrl = product.MockupUrl,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Variants = product.Variants.Select(v => new ProductVariantResponse
            {
                Id = v.Id,
                Size = v.Size,
                Color = v.Color,
                Sku = v.Sku,
                Price = v.Price,
                Stock = v.Stock,
                IsAvailable = v.IsAvailable,
                QikinkSku = v.QikinkSku
            }).ToList(),
            Images = product.Images.Select(i => new ProductImageResponse
            {
                Id = i.Id,
                Url = i.Url,
                AltText = i.AltText,
                SortOrder = i.SortOrder,
                IsFeatured = i.IsFeatured
            }).ToList()
        };
    }

    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            ShortDescription = request.ShortDescription,
            Sku = request.Sku,
            Price = request.Price,
            ComparePrice = request.ComparePrice,
            CostPrice = request.CostPrice,
            Brand = request.Brand,
            Tags = request.Tags,
            IsFeatured = request.IsFeatured,
            IsPublished = request.IsPublished,
            CategoryId = request.CategoryId,
            SeoTitle = request.SeoTitle,
            SeoDescription = request.SeoDescription,
            IsQikinkProduct = request.IsQikinkProduct,
            QikinkProductId = request.QikinkProductId?.Trim(),
            QikinkProductName = request.QikinkProductName?.Trim(),
            DesignReference = request.DesignReference?.Trim(),
            DesignFileUrl = request.DesignFileUrl?.Trim(),
            MockupUrl = request.MockupUrl?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Products.Add(product);

        var savedVariants = new List<ProductVariant>();
        foreach (var variant in request.Variants)
        {
            var productVariant = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Size = variant.Size,
                Color = variant.Color,
                Sku = variant.Sku,
                Price = variant.Price,
                Stock = variant.Stock,
                IsAvailable = variant.IsAvailable,
                QikinkSku = variant.QikinkSku?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            savedVariants.Add(productVariant);
            _context.ProductVariants.Add(productVariant);
        }

        foreach (var image in request.Images)
        {
            _context.ProductImages.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Url = image.Url,
                CloudinaryPublicId = CloudinaryUrlHelper.ExtractPublicId(image.Url),
                AltText = image.AltText,
                SortOrder = image.SortOrder,
                IsFeatured = image.IsFeatured,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await _context.SaveChangesAsync();

        if (product.IsQikinkProduct)
        {
            await SyncFulfillmentMappingsAsync(product.Id, savedVariants, product);
        }

        var categoryName = request.CategoryId.HasValue
            ? (await _context.Categories.FindAsync(request.CategoryId.Value))?.Name
            : null;

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            ShortDescription = product.ShortDescription,
            Sku = product.Sku,
            Price = product.Price,
            ComparePrice = product.ComparePrice,
            CostPrice = product.CostPrice,
            Brand = product.Brand,
            Tags = product.Tags,
            IsFeatured = product.IsFeatured,
            IsPublished = product.IsPublished,
            IsActive = product.IsActive,
            SeoTitle = product.SeoTitle,
            SeoDescription = product.SeoDescription,
            IsQikinkProduct = product.IsQikinkProduct,
            QikinkProductId = product.QikinkProductId,
            QikinkProductName = product.QikinkProductName,
            DesignReference = product.DesignReference,
            DesignFileUrl = product.DesignFileUrl,
            MockupUrl = product.MockupUrl,
            CategoryId = product.CategoryId,
            CategoryName = categoryName,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    public async Task<ProductResponse?> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _context.Products
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null) return null;

        product.Name = request.Name;
        product.Slug = request.Slug;
        product.Description = request.Description;
        product.ShortDescription = request.ShortDescription;
        product.Sku = request.Sku;
        product.Price = request.Price;
        product.ComparePrice = request.ComparePrice;
        product.CostPrice = request.CostPrice;
        product.Brand = request.Brand;
        product.Tags = request.Tags;
        product.IsFeatured = request.IsFeatured;
        product.IsPublished = request.IsPublished;
        product.CategoryId = request.CategoryId;
        product.SeoTitle = request.SeoTitle;
        product.SeoDescription = request.SeoDescription;
        product.IsQikinkProduct = request.IsQikinkProduct;
        product.QikinkProductId = request.QikinkProductId?.Trim();
        product.QikinkProductName = request.QikinkProductName?.Trim();
        product.DesignReference = request.DesignReference?.Trim();
        product.DesignFileUrl = request.DesignFileUrl?.Trim();
        product.MockupUrl = request.MockupUrl?.Trim();

        var existingVariants = product.Variants.ToList();
        foreach (var variant in existingVariants)
        {
            variant.IsActive = false;
        }

        var savedVariants = new List<ProductVariant>();
        foreach (var variant in request.Variants)
        {
            var productVariant = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Size = variant.Size,
                Color = variant.Color,
                Sku = variant.Sku,
                Price = variant.Price,
                Stock = variant.Stock,
                IsAvailable = variant.IsAvailable,
                QikinkSku = variant.QikinkSku?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            savedVariants.Add(productVariant);
            _context.ProductVariants.Add(productVariant);
        }

        var existingImages = product.Images.ToList();
        foreach (var image in existingImages)
        {
            image.IsActive = false;
        }

        foreach (var image in request.Images)
        {
            _context.ProductImages.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Url = image.Url,
                CloudinaryPublicId = CloudinaryUrlHelper.ExtractPublicId(image.Url),
                AltText = image.AltText,
                SortOrder = image.SortOrder,
                IsFeatured = image.IsFeatured,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await _context.SaveChangesAsync();

        await SyncFulfillmentMappingsAsync(product.Id, savedVariants, product);

        return await GetProductByIdAsync(id);
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.IsActive) return false;

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        var mappings = await _context.ProductFulfillmentMappings
            .Where(pfm => pfm.ProductId == id && pfm.IsActive)
            .ToListAsync();
        foreach (var mapping in mappings)
        {
            mapping.IsActive = false;
            mapping.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleProductPublishAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.IsActive) return false;

        product.IsPublished = !product.IsPublished;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PaginatedResponse<CategoryResponse>> GetCategoriesAsync(PaginatedRequest request)
    {
        var query = _context.Categories
            .Include(c => c.Parent)
            .Include(c => c.Products)
            .Where(c => c.IsActive && c.ParentId == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        query = query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                DisplayOrder = c.DisplayOrder,
                ParentId = c.ParentId,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count(p => p.IsActive),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<CategoryResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<CategoryResponse?> GetCategoryByIdAsync(Guid id)
    {
        var category = await _context.Categories
            .Include(c => c.Parent)
            .Include(c => c.Children.Where(ch => ch.IsActive))
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (category == null) return null;

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            ParentId = category.ParentId,
            ParentName = category.Parent?.Name,
            IsActive = category.IsActive,
            ProductCount = category.Products.Count(p => p.IsActive),
            CreatedAt = category.CreatedAt,
            Children = category.Children.Select(ch => new CategoryResponse
            {
                Id = ch.Id,
                Name = ch.Name,
                Slug = ch.Slug,
                Description = ch.Description,
                ImageUrl = ch.ImageUrl,
                DisplayOrder = ch.DisplayOrder,
                ParentId = ch.ParentId,
                IsActive = ch.IsActive,
                ProductCount = ch.Products.Count(p => p.IsActive),
                CreatedAt = ch.CreatedAt
            }).ToList()
        };
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            DisplayOrder = request.DisplayOrder,
            ParentId = request.ParentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            ParentId = category.ParentId,
            IsActive = category.IsActive,
            ProductCount = 0,
            CreatedAt = category.CreatedAt
        };
    }

    public async Task<CategoryResponse?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null || !category.IsActive) return null;

        category.Name = request.Name;
        category.Slug = request.Slug;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.DisplayOrder = request.DisplayOrder;
        category.ParentId = request.ParentId;

        await _context.SaveChangesAsync();

        return await GetCategoryByIdAsync(id);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (category == null) return false;

        if (category.Products.Any(p => p.IsActive) || category.Children.Any(c => c.IsActive))
            return false;

        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PaginatedResponse<CollectionResponse>> GetCollectionsAsync(PaginatedRequest request)
    {
        var query = _context.Collections
            .Include(c => c.CollectionProducts)
            .Where(c => c.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            _ => query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CollectionResponse
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                IsFeatured = c.IsFeatured,
                IsActive = c.IsActive,
                DisplayOrder = c.DisplayOrder,
                ProductCount = c.CollectionProducts.Count(cp => cp.Product.IsActive),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<CollectionResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<CollectionResponse?> GetCollectionByIdAsync(Guid id)
    {
        var collection = await _context.Collections
            .Include(c => c.CollectionProducts)
                .ThenInclude(cp => cp.Product)
                    .ThenInclude(p => p.Images.Where(i => i.IsFeatured))
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (collection == null) return null;

        return new CollectionResponse
        {
            Id = collection.Id,
            Name = collection.Name,
            Slug = collection.Slug,
            Description = collection.Description,
            ImageUrl = collection.ImageUrl,
            IsFeatured = collection.IsFeatured,
            IsActive = collection.IsActive,
            DisplayOrder = collection.DisplayOrder,
            ProductCount = collection.CollectionProducts.Count(cp => cp.Product.IsActive),
            CreatedAt = collection.CreatedAt,
            Products = collection.CollectionProducts
                .Where(cp => cp.Product.IsActive)
                .OrderBy(cp => cp.SortOrder)
                .Select(cp => new ProductBriefResponse
                {
                    Id = cp.Product.Id,
                    Name = cp.Product.Name,
                    Slug = cp.Product.Slug,
                    Price = cp.Product.Price,
                    ImageUrl = cp.Product.Images.FirstOrDefault(i => i.IsFeatured) != null
                        ? cp.Product.Images.First(i => i.IsFeatured).Url
                        : null
                }).ToList()
        };
    }

    public async Task<CollectionResponse> CreateCollectionAsync(CreateCollectionRequest request)
    {
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            IsFeatured = request.IsFeatured,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Collections.Add(collection);

        int sortOrder = 0;
        foreach (var productId in request.ProductIds)
        {
            _context.CollectionProducts.Add(new CollectionProduct
            {
                Id = Guid.NewGuid(),
                CollectionId = collection.Id,
                ProductId = productId,
                SortOrder = sortOrder++,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await _context.SaveChangesAsync();

        return await GetCollectionByIdAsync(collection.Id) ?? throw new InvalidOperationException("Failed to retrieve created collection");
    }

    public async Task<CollectionResponse?> UpdateCollectionAsync(Guid id, UpdateCollectionRequest request)
    {
        var collection = await _context.Collections
            .Include(c => c.CollectionProducts)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (collection == null) return null;

        collection.Name = request.Name;
        collection.Slug = request.Slug;
        collection.Description = request.Description;
        collection.ImageUrl = request.ImageUrl;
        collection.IsFeatured = request.IsFeatured;
        collection.DisplayOrder = request.DisplayOrder;

        var existingLinks = collection.CollectionProducts.ToList();
        _context.CollectionProducts.RemoveRange(existingLinks);

        int sortOrder = 0;
        foreach (var productId in request.ProductIds)
        {
            _context.CollectionProducts.Add(new CollectionProduct
            {
                Id = Guid.NewGuid(),
                CollectionId = collection.Id,
                ProductId = productId,
                SortOrder = sortOrder++,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await _context.SaveChangesAsync();

        return await GetCollectionByIdAsync(id);
    }

    public async Task<bool> DeleteCollectionAsync(Guid id)
    {
        var collection = await _context.Collections.FindAsync(id);
        if (collection == null || !collection.IsActive) return false;

        collection.IsActive = false;
        collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PaginatedResponse<OrderResponse>> GetOrdersAsync(PaginatedRequest request)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .Where(o => o.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(o => o.OrderNumber.ToLower().Contains(search) ||
                o.User.Email.ToLower().Contains(search) ||
                (o.User.FirstName + " " + o.User.LastName).ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        query = request.SortBy?.ToLower() switch
        {
            "total" => request.SortDescending ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
            "status" => request.SortDescending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
            _ => query.OrderByDescending(o => o.CreatedAt)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderResponse
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                UserId = o.UserId,
                CustomerName = o.User.FirstName + " " + o.User.LastName,
                CustomerEmail = o.User.Email,
                Status = o.Status,
                SubTotal = o.SubTotal,
                TaxAmount = o.TaxAmount,
                ShippingAmount = o.ShippingAmount,
                DiscountAmount = o.DiscountAmount,
                TotalAmount = o.TotalAmount,
                Currency = o.Currency,
                ShippingName = o.ShippingName,
                ShippingAddress = o.ShippingAddress,
                ShippingCity = o.ShippingCity,
                ShippingState = o.ShippingState,
                ShippingPostalCode = o.ShippingPostalCode,
                ShippingCountry = o.ShippingCountry,
                ShippingPhone = o.ShippingPhone,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                PaymentId = o.PaymentId,
                Notes = o.Notes,
                InternalNotes = o.InternalNotes,
                CreatedAt = o.CreatedAt,
                ShippedAt = o.ShippedAt,
                DeliveredAt = o.DeliveredAt,
                Items = o.Items.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Sku = i.Sku,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            })
            .ToListAsync();

        return new PaginatedResponse<OrderResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

        if (order == null) return null;

        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            CustomerName = order.User.FirstName + " " + order.User.LastName,
            CustomerEmail = order.User.Email,
            Status = order.Status,
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            ShippingAmount = order.ShippingAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            ShippingName = order.ShippingName,
            ShippingAddress = order.ShippingAddress,
            ShippingCity = order.ShippingCity,
            ShippingState = order.ShippingState,
            ShippingPostalCode = order.ShippingPostalCode,
            ShippingCountry = order.ShippingCountry,
            ShippingPhone = order.ShippingPhone,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            PaymentId = order.PaymentId,
            Notes = order.Notes,
            InternalNotes = order.InternalNotes,
            CreatedAt = order.CreatedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Sku = i.Sku,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                Size = i.ProductVariant?.Size,
                Color = i.ProductVariant?.Color
            }).ToList()
        };
    }

    public async Task<OrderResponse?> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null || !order.IsActive) return null;

        order.Status = request.Status;
        if (request.InternalNotes != null) order.InternalNotes = request.InternalNotes;
        if (request.Status == OrderStatus.Shipped) order.ShippedAt = DateTime.UtcNow;
        if (request.Status == OrderStatus.Delivered) order.DeliveredAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(id);
    }

    public async Task<PaginatedResponse<CustomerResponse>> GetCustomersAsync(PaginatedRequest request)
    {
        var query = _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => u.IsActive && !u.IsAdmin)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(u => u.Email.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "email" => request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        var customerIds = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => u.Id)
            .ToListAsync();

        var customers = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => customerIds.Contains(u.Id))
            .ToListAsync();

        var orderCounts = await _context.Orders
            .Where(o => customerIds.Contains(o.UserId) && o.IsActive)
            .GroupBy(o => o.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count(), Total = g.Sum(o => o.TotalAmount) })
            .ToListAsync();

        var result = customers.Select(u =>
        {
            var orderInfo = orderCounts.FirstOrDefault(o => o.UserId == u.Id);
            return new CustomerResponse
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsEmailVerified = u.IsEmailVerified,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                OrderCount = orderInfo?.Count ?? 0,
                TotalSpent = orderInfo?.Total ?? 0,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
            };
        }).ToList();

        return new PaginatedResponse<CustomerResponse>
        {
            Items = result,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<CustomerDetailResponse?> GetCustomerByIdAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive && !u.IsAdmin);

        if (user == null) return null;

        var orders = await _context.Orders
            .Where(o => o.UserId == id && o.IsActive)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new OrderResponse
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        var orderStats = await _context.Orders
            .Where(o => o.UserId == id && o.IsActive)
            .GroupBy(o => o.UserId)
            .Select(g => new { Count = g.Count(), Total = g.Sum(o => o.TotalAmount) })
            .FirstOrDefaultAsync();

        return new CustomerDetailResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsEmailVerified = user.IsEmailVerified,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            OrderCount = orderStats?.Count ?? 0,
            TotalSpent = orderStats?.Total ?? 0,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            RecentOrders = orders
        };
    }

    public async Task<bool> ToggleCustomerActiveAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.IsAdmin) return false;

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PaginatedResponse<CouponResponse>> GetCouponsAsync(PaginatedRequest request)
    {
        var query = _context.Coupons.Where(c => c.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(c => c.Code.ToLower().Contains(search) || (c.Description != null && c.Description.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync();

        query = request.SortBy?.ToLower() switch
        {
            "code" => request.SortDescending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "value" => request.SortDescending ? query.OrderByDescending(c => c.Value) : query.OrderBy(c => c.Value),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CouponResponse
            {
                Id = c.Id,
                Code = c.Code,
                Description = c.Description,
                Type = c.Type,
                Value = c.Value,
                MinimumOrderAmount = c.MinimumOrderAmount,
                MaximumDiscountAmount = c.MaximumDiscountAmount,
                UsageLimit = c.UsageLimit,
                UsedCount = c.UsedCount,
                ExpiresAt = c.ExpiresAt,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<CouponResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<CouponResponse?> GetCouponByIdAsync(Guid id)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        if (coupon == null) return null;

        return new CouponResponse
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Description = coupon.Description,
            Type = coupon.Type,
            Value = coupon.Value,
            MinimumOrderAmount = coupon.MinimumOrderAmount,
            MaximumDiscountAmount = coupon.MaximumDiscountAmount,
            UsageLimit = coupon.UsageLimit,
            UsedCount = coupon.UsedCount,
            ExpiresAt = coupon.ExpiresAt,
            IsActive = coupon.IsActive,
            CreatedAt = coupon.CreatedAt
        };
    }

    public async Task<CouponResponse> CreateCouponAsync(CreateCouponRequest request)
    {
        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = request.Code.ToUpper(),
            Description = request.Description,
            Type = request.Type,
            Value = request.Value,
            MinimumOrderAmount = request.MinimumOrderAmount,
            MaximumDiscountAmount = request.MaximumDiscountAmount,
            UsageLimit = request.UsageLimit,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        return new CouponResponse
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Description = coupon.Description,
            Type = coupon.Type,
            Value = coupon.Value,
            MinimumOrderAmount = coupon.MinimumOrderAmount,
            MaximumDiscountAmount = coupon.MaximumDiscountAmount,
            UsageLimit = coupon.UsageLimit,
            UsedCount = coupon.UsedCount,
            ExpiresAt = coupon.ExpiresAt,
            IsActive = coupon.IsActive,
            CreatedAt = coupon.CreatedAt
        };
    }

    public async Task<CouponResponse?> UpdateCouponAsync(Guid id, UpdateCouponRequest request)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon == null || !coupon.IsActive) return null;

        coupon.Code = request.Code.ToUpper();
        coupon.Description = request.Description;
        coupon.Type = request.Type;
        coupon.Value = request.Value;
        coupon.MinimumOrderAmount = request.MinimumOrderAmount;
        coupon.MaximumDiscountAmount = request.MaximumDiscountAmount;
        coupon.UsageLimit = request.UsageLimit;
        coupon.ExpiresAt = request.ExpiresAt;

        await _context.SaveChangesAsync();

        return await GetCouponByIdAsync(id);
    }

    public async Task<bool> DeleteCouponAsync(Guid id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon == null || !coupon.IsActive) return false;

        coupon.IsActive = false;
        coupon.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PaginatedResponse<ReviewResponse>> GetReviewsAsync(PaginatedRequest request, bool? isApproved = null)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.IsActive)
            .AsQueryable();

        if (isApproved.HasValue)
            query = query.Where(r => r.IsApproved == isApproved.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(search) || r.Product.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        query = request.SortBy?.ToLower() switch
        {
            "rating" => request.SortDescending ? query.OrderByDescending(r => r.Rating) : query.OrderBy(r => r.Rating),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User.FirstName + " " + r.User.LastName,
                UserEmail = r.User.Email,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                IsApproved = r.IsApproved,
                IsHidden = r.IsHidden,
                AdminReply = r.AdminReply,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<ReviewResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<ReviewResponse?> UpdateReviewAsync(Guid id, UpdateReviewRequest request)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

        if (review == null) return null;

        review.IsApproved = request.IsApproved;
        review.IsHidden = request.IsHidden;

        await _context.SaveChangesAsync();

        return new ReviewResponse
        {
            Id = review.Id,
            UserId = review.UserId,
            UserName = review.User.FirstName + " " + review.User.LastName,
            UserEmail = review.User.Email,
            ProductId = review.ProductId,
            ProductName = review.Product.Name,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            IsApproved = review.IsApproved,
            IsHidden = review.IsHidden,
            AdminReply = review.AdminReply,
            CreatedAt = review.CreatedAt
        };
    }

    public async Task<ReviewResponse?> ReplyToReviewAsync(Guid id, ReplyReviewRequest request)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

        if (review == null) return null;

        review.AdminReply = request.AdminReply;

        await _context.SaveChangesAsync();

        return new ReviewResponse
        {
            Id = review.Id,
            UserId = review.UserId,
            UserName = review.User.FirstName + " " + review.User.LastName,
            UserEmail = review.User.Email,
            ProductId = review.ProductId,
            ProductName = review.Product.Name,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            IsApproved = review.IsApproved,
            IsHidden = review.IsHidden,
            AdminReply = review.AdminReply,
            CreatedAt = review.CreatedAt
        };
    }

    public async Task<bool> DeleteReviewAsync(Guid id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null || !review.IsActive) return false;

        review.IsActive = false;
        review.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<StoreSettingsResponse?> GetSettingsAsync()
    {
        var settings = await _context.StoreSettings.FirstOrDefaultAsync();
        if (settings == null) return null;

        return MapSettings(settings);
    }

    public async Task<StoreSettingsResponse> UpdateSettingsAsync(UpdateStoreSettingsRequest request)
    {
        var settings = await _context.StoreSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new StoreSettings
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.StoreSettings.Add(settings);
        }

        settings.StoreName = request.StoreName;
        settings.StoreDescription = request.StoreDescription;
        settings.ContactEmail = request.ContactEmail;
        settings.ContactPhone = request.ContactPhone;
        settings.Address = request.Address;
        settings.Currency = request.Currency;
        settings.CurrencySymbol = request.CurrencySymbol;
        settings.TaxRate = request.TaxRate;
        settings.ShippingPolicy = request.ShippingPolicy;
        settings.ReturnPolicy = request.ReturnPolicy;
        settings.PrivacyPolicy = request.PrivacyPolicy;
        settings.TermsOfService = request.TermsOfService;
        settings.LogoUrl = request.LogoUrl;
        settings.FaviconUrl = request.FaviconUrl;
        settings.PrimaryColor = request.PrimaryColor;
        settings.SocialFacebook = request.SocialFacebook;
        settings.SocialInstagram = request.SocialInstagram;
        settings.SocialTwitter = request.SocialTwitter;
        settings.SocialYoutube = request.SocialYoutube;
        settings.RazorpayKeyId = request.RazorpayKeyId;
        if (!string.IsNullOrEmpty(request.RazorpayKeySecret))
            settings.RazorpayKeySecret = request.RazorpayKeySecret;
        settings.CloudinaryCloudName = request.CloudinaryCloudName;
        settings.CloudinaryApiKey = request.CloudinaryApiKey;
        if (!string.IsNullOrEmpty(request.CloudinaryApiSecret))
            settings.CloudinaryApiSecret = request.CloudinaryApiSecret;

        await _context.SaveChangesAsync();

        return MapSettings(settings);
    }

    public async Task<PaginatedResponse<ProductResponse>> GetInventoryAsync(PaginatedRequest request)
    {
        var query = _context.Products
            .Include(p => p.Variants.Where(v => v.IsActive))
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) || p.Sku.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Sku = p.Sku,
                Price = p.Price,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<ProductResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<bool> UpdateInventoryAsync(Guid variantId, int stock)
    {
        if (stock < 0) return false;

        var variant = await _context.ProductVariants.FindAsync(variantId);
        if (variant == null || !variant.IsActive) return false;

        variant.Stock = stock;
        variant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    private static StoreSettingsResponse MapSettings(StoreSettings settings)
    {
        return new StoreSettingsResponse
        {
            Id = settings.Id,
            StoreName = settings.StoreName,
            StoreDescription = settings.StoreDescription,
            ContactEmail = settings.ContactEmail,
            ContactPhone = settings.ContactPhone,
            Address = settings.Address,
            Currency = settings.Currency,
            CurrencySymbol = settings.CurrencySymbol,
            TaxRate = settings.TaxRate,
            ShippingPolicy = settings.ShippingPolicy,
            ReturnPolicy = settings.ReturnPolicy,
            PrivacyPolicy = settings.PrivacyPolicy,
            TermsOfService = settings.TermsOfService,
            LogoUrl = settings.LogoUrl,
            FaviconUrl = settings.FaviconUrl,
            PrimaryColor = settings.PrimaryColor,
            SocialFacebook = settings.SocialFacebook,
            SocialInstagram = settings.SocialInstagram,
            SocialTwitter = settings.SocialTwitter,
            SocialYoutube = settings.SocialYoutube,
            RazorpayKeyId = settings.RazorpayKeyId,
            HasRazorpayKeySecret = !string.IsNullOrEmpty(settings.RazorpayKeySecret),
            CloudinaryCloudName = settings.CloudinaryCloudName,
            CloudinaryApiKey = settings.CloudinaryApiKey,
            HasCloudinaryApiSecret = !string.IsNullOrEmpty(settings.CloudinaryApiSecret)
        };
    }

    private async Task SyncFulfillmentMappingsAsync(Guid productId, List<ProductVariant> variants, Product product)
    {
        var provider = await _context.FulfillmentProviders
            .FirstOrDefaultAsync(p => p.Name == "Qikink" && p.IsActive);

        if (provider == null) return;

        var existingMappings = await _context.ProductFulfillmentMappings
            .Where(pfm => pfm.ProductId == productId && pfm.ProviderId == provider.Id)
            .ToListAsync();

        if (!product.IsQikinkProduct)
        {
            foreach (var mapping in existingMappings)
            {
                mapping.IsActive = false;
                mapping.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            return;
        }

        var externalProductId = product.QikinkProductId ?? string.Empty;

        foreach (var mapping in existingMappings)
        {
            mapping.IsActive = false;
            mapping.UpdatedAt = DateTime.UtcNow;
        }

        foreach (var variant in variants.Where(v => !string.IsNullOrEmpty(v.QikinkSku)))
        {
            var existingMapping = existingMappings.FirstOrDefault(m => m.ProductVariantId == variant.Id);
            if (existingMapping != null)
            {
                existingMapping.IsActive = true;
                existingMapping.ExternalProductId = externalProductId;
                existingMapping.ExternalSku = variant.QikinkSku;
                existingMapping.ExternalVariantId = null;
                existingMapping.DesignReference = product.DesignReference;
                existingMapping.DesignFileUrl = product.DesignFileUrl;
                existingMapping.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.ProductFulfillmentMappings.Add(new ProductFulfillmentMapping
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ProductVariantId = variant.Id,
                    ProviderId = provider.Id,
                    ExternalProductId = externalProductId,
                    ExternalSku = variant.QikinkSku,
                    DesignReference = product.DesignReference,
                    DesignFileUrl = product.DesignFileUrl,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
    }
}
