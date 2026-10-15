namespace KENNArchive.Domain.Entities;
// İstifadəçinin səbətini saxlayır.
public class Cart : BaseEntity
{
    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }
    // Səbətdəki məhsullar.
    public ICollection<CartItem> Items { get; set; }
        = new List<CartItem>();
}