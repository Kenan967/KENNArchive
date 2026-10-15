using KENNArchive.Application.DTOs;
using KENNArchive.Application.DTOs.Product;
using KENNArchive.Application.Interfaces;
using KENNArchive.Domain.Entities;
using KENNArchive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KENNArchive.Infrastructure.Services;

// Məhsullar üzrə database əməliyyatlarını yerinə yetirir.
public class ProductService : IProductService
{
    private readonly KennArchiveDbContext _context;

    public ProductService(KennArchiveDbContext context)
    {
        _context = context;
    }

    // Məhsulları axtarış, brend, kateqoriya, qiymət,
    // sıralama və pagination ilə gətirir.
    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(
        string? search,
        string? category,
        string? brand,
        decimal? minPrice,
        decimal? maxPrice,
        string? sort,
        int page,
        int pageSize)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.Variants)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Name.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            category = category.Trim();

            query = query.Where(x =>
                x.Category != null &&
                x.Category.Slug == category);
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            brand = brand.Trim();

            query = query.Where(x =>
                x.Brand != null &&
                x.Brand.Slug == brand);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(x =>
                x.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(x =>
                x.Price <= maxPrice.Value);
        }

        query = sort?.ToLowerInvariant() switch
        {
            "priceasc" => query.OrderBy(x => x.Price),
            "pricedesc" => query.OrderByDescending(x => x.Price),
            "newest" => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderBy(x => x.Name)
        };

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return products.Select(x => new ProductDto
        {
            Name = x.Name,
            Slug = x.Slug,
            Description = x.Description,
            Price = x.Price,
            Stock = x.Stock,

            Category = x.Category?.Name ?? string.Empty,
            Brand = x.Brand?.Name ?? string.Empty,

            Sizes = x.Variants
                .OrderBy(v => v.Id)
                .Select(v => new ProductSizeDto
                {
                    Id = v.Id,
                    Size = v.Size,
                    Stock = v.Stock
                })
                .ToList()
        }).ToList();
    }

    // Yeni məhsul yaradır.
    public async Task<Product?> CreateAsync(
        CreateProductRequest request)
    {
        if (request.Stock < 0 || request.Price < 0)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return null;
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(x =>
                x.Id == request.CategoryId);

        if (category == null)
        {
            return null;
        }

        var brand = await _context.Brands
            .FirstOrDefaultAsync(x =>
                x.Id == request.BrandId);

        if (brand == null)
        {
            return null;
        }

        var slug = request.Name
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "-");

        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(x => x.Slug == slug);

        if (existingProduct != null)
        {
            return null;
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = category.Id,
            BrandId = brand.Id
        };

        // Yeni məhsul üçün bütün ölçülər yaradılır.
        // İlkin stok M ölçüsünə yazılır.
        var sizes = new[] { "XS", "S", "M", "L", "XL", "XXL" };

        foreach (var size in sizes)
        {
            product.Variants.Add(new ProductVariant
            {
                Size = size,
                Stock = size == "M" ? request.Stock : 0
            });
        }

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return product;
    }

    // Slug-a görə məhsulun detallarını gətirir.
    public async Task<ProductDetailDto?> GetBySlugAsync(
        string slug)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Slug == slug);

        if (product == null)
        {
            return null;
        }

        return new ProductDetailDto
        {
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,

            Category = product.Category?.Name ?? string.Empty,
            CategorySlug = product.Category?.Slug ?? string.Empty,

            Brand = product.Brand?.Name ?? string.Empty,
            BrandSlug = product.Brand?.Slug ?? string.Empty,

            Sizes = product.Variants
                .OrderBy(v => v.Id)
                .Select(v => new ProductSizeDto
                {
                    Id = v.Id,
                    Size = v.Size,
                    Stock = v.Stock
                })
                .ToList()
        };
    }

    // Mövcud məhsulun məlumatlarını yeniləyir.
    public async Task<bool> UpdateAsync(
        string slug,
        UpdateProductRequest request)
    {
        if (request.Stock < 0 || request.Price < 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return false;
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Slug == slug);

        if (product == null)
        {
            return false;
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(x =>
                x.Id == request.CategoryId);

        if (category == null)
        {
            return false;
        }

        var brand = await _context.Brands
            .FirstOrDefaultAsync(x =>
                x.Id == request.BrandId);

        if (brand == null)
        {
            return false;
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = category.Id;
        product.BrandId = brand.Id;

        await _context.SaveChangesAsync();

        return true;
    }

    // Slug-a görə məhsulu silir.
    public async Task<bool> DeleteAsync(string slug)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Slug == slug);

        if (product == null)
        {
            return false;
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return true;
    }

    // Slug vasitəsilə məhsulun database ID-sini tapır.
    public async Task<int?> GetIdBySlugAsync(string slug)
    {
        return await _context.Products
            .Where(x => x.Slug == slug)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
    }

    // Məhsulun konkret ölçüsünün stokunu yeniləyir.
    public async Task<bool> UpdateVariantStockAsync(
        string slug,
        int variantId,
        int stock)
    {
        if (stock < 0)
        {
            return false;
        }

        var variant = await _context.ProductVariants
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.Id == variantId &&
                x.Product != null &&
                x.Product.Slug == slug);

        if (variant == null)
        {
            return false;
        }

        variant.Stock = stock;

        await _context.SaveChangesAsync();

        return true;
    }
}