namespace KENNArchive.Application.DTOs.Order;


// İstifadəçinin sifariş yaradarkən göndərdiyi məlumatlar.
public class CreateOrderRequest
{


    public string ShippingAddress { get; set; } = string.Empty;
}