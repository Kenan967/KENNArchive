namespace KENNArchive.Application.DTOs;

// API-də entity-ni birbaşa qaytarmaq əvəzinə istifadə etdiyimiz sadə DTO.
public class ProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
}
