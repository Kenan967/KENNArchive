using KENNArchive.Application.DTOs;
using KENNArchive.Application.Interfaces;
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
            query = query.Where(x => x.Name.Contains(search));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(x => x.Category != null && x.Category.Slug == category);

        if (minPrice.HasValue)
            query = query.Where(x => x.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(x => x.Price <= maxPrice.Value);

        query = sort?.ToLowerInvariant() switch
        {
            "priceasc" => query.OrderBy(x => x.Price),
            "pricedesc" => query.OrderByDescending(x => x.Price),
            "newest" => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderBy(x => x.Name)
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
                Category = x.Category != null ? x.Category.Name : string.Empty,
                Brand = x.Brand != null ? x.Brand.Name : string.Empty
            })
            .ToListAsync();
    }
}
