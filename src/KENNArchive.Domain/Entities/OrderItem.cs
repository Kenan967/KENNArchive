namespace KENNArchive.Domain.Entities;
// Sifarişin içindəki hər bir məhsulu saxlayır.
public class OrderItem : BaseEntity
{
    // Hansı sifarişə aid olduğunu göstərir.
    public int OrderId { get; set; }

    public Order? Order { get; set; }

    // Sifariş edilən məhsul.
    public int ProductId { get; set; }

    public Product? Product { get; set; }

    // Məhsulun sifariş zamanı olan adı.
    public string ProductName { get; set; } = string.Empty;

    // Sifariş zamanı seçilmiş ölçü.
    // Sonradan məhsulun ölçüləri dəyişsə belə, sifarişdə qalır.
    public string Size { get; set; } = string.Empty;

    // Məhsulun sifariş zamanı olan qiyməti.
    public decimal UnitPrice { get; set; }

    // Sifariş edilən məhsulun sayı.
    public int Quantity { get; set; }

    // Bu məhsulların ümumi qiyməti.
    public decimal TotalPrice { get; set; }
}
