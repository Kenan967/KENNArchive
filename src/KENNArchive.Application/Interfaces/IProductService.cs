using KENNArchive.Application.DTOs;

namespace KENNArchive.Application.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(
        string? search,
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        string? sort,
        int page,
        int pageSize);
}
