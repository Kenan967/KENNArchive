namespace KENNArchive.Application.DTOs.Order;
// Sifarişdəki bir məhsulun məlumatları.
public class OrderItemDto
{

    public string ProductName { get; set; } = string.Empty;

    public string ProductSlug { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }


    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }
}