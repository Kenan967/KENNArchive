namespace KENNArchive.Application.DTOs.Product;

// Məhsulun konkret ölçüsünün stokunu dəyişmək üçün istifadə olunur.
public class UpdateProductVariantStockRequest
{
    public int Stock { get; set; }
}