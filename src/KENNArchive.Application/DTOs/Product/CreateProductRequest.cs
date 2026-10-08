namespace KENNArchive.Application.DTOs.Product;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public int Stock { get; set; }

    public int CategoryId { get; set; }
    public int BrandId { get; set; }
}