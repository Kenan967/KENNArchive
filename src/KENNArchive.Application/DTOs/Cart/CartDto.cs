namespace KENNArchive.Application.DTOs.Cart;

// İstifadəçinin bütün səbət məlumatları.
public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();

    public decimal TotalPrice { get; set; }
}