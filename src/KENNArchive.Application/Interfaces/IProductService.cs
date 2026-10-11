using KENNArchive.Application.DTOs;
using KENNArchive.Application.DTOs.Product;
using KENNArchive.Domain.Entities;

namespace KENNArchive.Application.Interfaces;

public interface IProductService
{
    // Məhsulları axtarış, brend, kateqoriya,
    // qiymət, sıralama və pagination ilə gətirir.
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(
    string? search,
    string? category,
    string? brand,
    decimal? minPrice,
    decimal? maxPrice,
    string? sort,
    int page,
    int pageSize);
// Yeni məhsul yaradır.
Task<Product?> CreateAsync(
    CreateProductRequest request);

    // Slug-a görə məhsulun detallarını gətirir.
    Task<ProductDetailDto?> GetBySlugAsync(
        string slug);

    // Mövcud məhsulu yeniləyir.
    Task<bool> UpdateAsync(
        string slug,
        UpdateProductRequest request);

    // Məhsulu silir.
    Task<bool> DeleteAsync(
        string slug);

    // Slug vasitəsilə məhsulun ID-sini tapır.
    Task<int?> GetIdBySlugAsync(
        string slug);

}
