using KENNArchive.Application.DTOs;
using KENNArchive.Application.DTOs.Product;
using KENNArchive.Domain.Entities;

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

    Task<Product?> CreateAsync(
        CreateProductRequest request);

    Task<ProductDetailDto?> GetBySlugAsync(
        string slug);

    Task<bool> UpdateAsync(
        string slug,
        UpdateProductRequest request);

    Task<bool> DeleteAsync(
        string slug);

    Task<int?> GetIdBySlugAsync(
        string slug);
}