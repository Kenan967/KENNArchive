namespace KENNArchive.Application.DTOs;

// API-də məhsul məlumatlarını ötürmək üçün istifadə edilən DTO.
public class ProductDto
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    // Köhnə sistemlə uyğunluq üçün saxlanılır.
    public int Stock { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    // Məhsulun ölçüləri və hər ölçünün mövcud stoku.
    public List<ProductSizeDto> Sizes { get; set; } = new();
}
