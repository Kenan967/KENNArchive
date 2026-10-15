namespace KENNArchive.Application.DTOs.Cart;

// Səbətə məhsul əlavə etmək üçün istifadə olunur.
public class AddCartItemRequest
{
    public string ProductSlug { get; set; } = string.Empty;

    // Müştərinin seçdiyi ölçünün ID-si.
    public int ProductVariantId { get; set; }

    public int Quantity { get; set; }
}