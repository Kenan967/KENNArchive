namespace KENNArchive.Application.DTOs.Wishlist;
// İstifadəçinin Wishlist məlumatları.
public class WishlistDto
{

    public List<WishlistItemDto> Items { get; set; } = new();
}