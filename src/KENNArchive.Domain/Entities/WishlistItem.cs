namespace KENNArchive.Domain.Entities;

// Wishlist-də saxlanılan bir məhsulu göstərir.
public class WishlistItem : BaseEntity
{
    public int WishlistId { get; set; }
    public Wishlist? Wishlist { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
}