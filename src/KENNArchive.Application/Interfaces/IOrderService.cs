using KENNArchive.Application.DTOs.Order;

namespace KENNArchive.Application.Interfaces;

// Sifariş sistemi üçün əsas əməliyyatlar.
public interface IOrderService
{
    // Səbətdəki məhsullardan yeni sifariş yaradır.
    Task<OrderDto?> CreateOrderAsync(
        int userId,
        CreateOrderRequest request);

    // İstifadəçinin bütün sifarişlərini qaytarır.
    Task<List<OrderDto>> GetMyOrdersAsync(int userId);

    // Bir sifarişin ətraflı məlumatlarını qaytarır.
    Task<OrderDto?> GetOrderByNumberAsync(
        int userId,
        string orderNumber);

    // Admin sifarişin statusunu dəyişir.
    Task<bool> UpdateOrderStatusAsync(
        string orderNumber,
        string status);
}