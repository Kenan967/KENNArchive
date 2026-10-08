namespace KENNArchive.Application.DTOs.Cart;

// Səbətə məhsul əlavə etmək üçün istifadə olunur.
public class AddCartItemRequest
{
    public string ProductSlug { get; set; } = string.Empty;

    public int Quantity { get; set; }
}