namespace KENNArchive.Domain.Entities;

// Səbətdəki bir Product-u və sayını saxlayır.
public class CartItem : BaseEntity
{
    public int CartId { get; set; }

    public Cart? Cart { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int Quantity { get; set; }
}