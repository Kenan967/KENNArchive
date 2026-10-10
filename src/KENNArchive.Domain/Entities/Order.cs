using System;
using System.Collections.Generic;
namespace KENNArchive.Domain.Entities;



// İstifadəçinin verdiyi sifarişi saxlayır.
public class Order : BaseEntity
{
    // Sifarişin sahibi
    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }
    // Sifarişin unikal nömrəsi
    public string OrderNumber { get; set; } = Guid.NewGuid().ToString("N");
    // Sifarişin ümumi məbləği
    public decimal TotalPrice { get; set; }
    // Sifarişin statusu
    public string Status { get; set; } = "Pending";
    // Çatdırılma ünvanı
    public string ShippingAddress { get; set; } = string.Empty;
    // Sifarişdəki məhsullar
    public ICollection<OrderItem> Items { get; set; }
        = new List<OrderItem>();
}