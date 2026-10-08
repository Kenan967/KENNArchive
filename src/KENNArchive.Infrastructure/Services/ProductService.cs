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

    // Məhsulları search, sorting, filter və pagination ilə gətirir.
    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(
        string? search,
        string? category,
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
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Name.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x =>
                x.Category != null &&
                x.Category.Slug == category);
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
            "priceasc" =>
                query.OrderBy(x => x.Price),

            "pricedesc" =>
                query.OrderByDescending(x => x.Price),

            "newest" =>
                query.OrderByDescending(x => x.CreatedAt),

            _ =>
                query.OrderBy(x => x.Name)
        };

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductDto
            {
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                Price = x.Price,
                Stock = x.Stock,

                Category = x.Category != null
                    ? x.Category.Name
                    : string.Empty,

                Brand = x.Brand != null
                    ? x.Brand.Name
                    : string.Empty
            })
            .ToListAsync();
    }

    // Yeni Product yaradır.
    public async Task<Product?> CreateAsync(
        CreateProductRequest request)
    {
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
            .ToLower()
            .Replace(" ", "-");

        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Slug == slug);

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
            CategoryId = request.CategoryId,
            BrandId = request.BrandId
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return product;
    }

    // Slug-a görə bir Product-un detallarını gətirir.
    public async Task<ProductDetailDto?> GetBySlugAsync(
        string slug)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Where(x => x.Slug == slug)
            .Select(x => new ProductDetailDto
            {
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                Price = x.Price,
                Stock = x.Stock,

                Category = x.Category != null
                    ? x.Category.Name
                    : string.Empty,

                CategorySlug = x.Category != null
                    ? x.Category.Slug
                    : string.Empty,

                Brand = x.Brand != null
                    ? x.Brand.Name
                    : string.Empty,

                BrandSlug = x.Brand != null
                    ? x.Brand.Slug
                    : string.Empty
            })
            .FirstOrDefaultAsync();
    }

    // Mövcud Product məlumatlarını yeniləyir.
    public async Task<bool> UpdateAsync(
        string slug,
        UpdateProductRequest request)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Slug == slug);

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
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;

        await _context.SaveChangesAsync();

        return true;
    }

    // Slug-a görə Product-u silir.
    public async Task<bool> DeleteAsync(string slug)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Slug == slug);

        if (product == null)
        {
            return false;
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return true;
    }

    // Public slug-dan database Product ID-sini tapır.
    // ID-ni URL-də göstərmirik, yalnız daxildə istifadə edirik.
    public async Task<int?> GetIdBySlugAsync(string slug)
    {
        return await _context.Products
            .Where(x => x.Slug == slug)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
    }
}