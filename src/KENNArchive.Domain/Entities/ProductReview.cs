namespace KENNArchive.Domain.Entities;

public class ProductReview : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;
}