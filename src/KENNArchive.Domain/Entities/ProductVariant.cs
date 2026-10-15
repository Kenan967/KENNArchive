namespace KENNArchive.Domain.Entities;
// Məhsulun konkret ölçüsünü və həmin ölçünün stokunu saxlayır.
public class ProductVariant : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    // Məsələn: XS, S, M, L, XL, XXL.
    public string Size { get; set; } = string.Empty;
    // Yalnız bu ölçüyə aid stok.
    public int Stock { get; set; }
}

