namespace KENNArchive.Application.DTOs.Wishlist;

// Wishlist-dəki məhsulun frontend-ə göndərilən məlumatları.
public class WishlistItemDto
{
    public string ProductName { get; set; } = string.Empty;

    public string ProductSlug { get; set; } = string.Empty;

    public decimal Price { get; set; }
}