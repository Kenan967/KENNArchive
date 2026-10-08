namespace KENNArchive.Application.DTOs.Cart;

// Səbətdəki məhsulun frontend-ə göndərilən məlumatları.
public class CartItemDto
{
    public string ProductName { get; set; } = string.Empty;

    public string ProductSlug { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }
}