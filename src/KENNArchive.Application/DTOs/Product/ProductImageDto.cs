namespace KENNArchive.Application.DTOs.Product;

// Product şəklinin frontend-ə göndərilən məlumatları.
public class ProductImageDto
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string PublicId { get; set; } = string.Empty;
}