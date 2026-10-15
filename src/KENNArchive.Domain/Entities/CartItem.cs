namespace KENNArchive.Domain.Entities;

// Səbətdəki məhsulu, ölçüsünü və sayını saxlayır.
public class CartItem : BaseEntity
{
    public int CartId { get; set; }

    public Cart? Cart { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    // Müştərinin seçdiyi ölçünün ID-si.
    // Nullable saxlanılır ki, köhnə səbət məlumatları qorunsun.
    public int? ProductVariantId { get; set; }

    public ProductVariant? ProductVariant { get; set; }

    // Səbətə əlavə edilən məhsulun sayı.
    public int Quantity { get; set; }
}