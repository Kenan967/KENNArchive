namespace KENNArchive.Domain.Entities;

// Mağazada satılan geyim məhsulunu təmsil edir.
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }

    // Köhnə sistemlə uyğunluq üçün hələlik saxlayırıq.
    public int Stock { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int BrandId { get; set; }
    public Brand? Brand { get; set; }

    public ICollection<ProductImage> Images { get; set; }
        = new List<ProductImage>();

    // Məhsulun ölçüləri və hər ölçünün ayrıca stoku.
    public ICollection<ProductVariant> Variants { get; set; }
        = new List<ProductVariant>();
}
