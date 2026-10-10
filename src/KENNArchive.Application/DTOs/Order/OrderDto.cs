namespace KENNArchive.Application.DTOs.Order;

// Sifariş haqqında ümumi məlumatlar.
public class OrderDto
{
    public string OrderNumber { get; set; } = string.Empty;

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = string.Empty;

    public string ShippingAddress { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();
}