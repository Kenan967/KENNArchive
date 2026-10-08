namespace KENNArchive.Domain.Entities;

// İstifadəçinin Wishlist məlumatını saxlayır.
public class Wishlist : BaseEntity
{
    public int UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public ICollection<WishlistItem> Items { get; set; }
        = new List<WishlistItem>();
}