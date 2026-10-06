namespace KENNArchive.Domain.Entities;

// Şəkilin özü database-də saxlanmır.
// Yalnız Cloudinary URL-i və həmin faylın PublicId-si saxlanılır.
public class ProductImage : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public Product? Product { get; set; }
}
