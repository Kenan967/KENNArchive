namespace KENNArchive.Application.DTOs.Product;
public class ProductDetailDto
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string BrandSlug { get; set; } = string.Empty;
}