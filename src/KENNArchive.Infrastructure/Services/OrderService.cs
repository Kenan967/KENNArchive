using KENNArchive.Application.DTOs.Order;
using KENNArchive.Application.Interfaces;
using KENNArchive.Domain.Entities;
using KENNArchive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KENNArchive.Infrastructure.Services;

// Sifarişlərin yaradılması və idarə olunması.
public class OrderService : IOrderService
{
    private readonly KennArchiveDbContext _context;

public OrderService(KennArchiveDbContext context)
    {
        _context = context;
    }

    // Səbətdəki məhsullardan yeni sifariş yaradır.
    public async Task<OrderDto?> CreateOrderAsync(
        int userId,
        CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
        {
            return null;
        }

        var cart = await _context.Carts
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            return null;
        }

        // Sifarişə başlamazdan əvvəl məhsulların stokunu yoxlayırıq.
        foreach (var item in cart.Items)
        {
            if (item.Product == null ||
                item.Quantity <= 0 ||
                item.Product.Stock < item.Quantity)
            {
                return null;
            }
        }

        // Sifariş və stok dəyişikliklərini birlikdə icra edirik.
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var order = new KENNArchive.Domain.Entities.Order
            {
                UserId = userId,
                OrderNumber = Guid.NewGuid().ToString("N"),
                ShippingAddress = request.ShippingAddress.Trim(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            foreach (var cartItem in cart.Items)
            {
                var product = cartItem.Product!;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = cartItem.Quantity,
                    TotalPrice = product.Price * cartItem.Quantity
                };

                order.Items.Add(orderItem);

                // Sifariş edilən say qədər stokdan çıxırıq.
                product.Stock -= cartItem.Quantity;

                // Sifarişin ümumi qiymətini hesablayırıq.
                order.TotalPrice += orderItem.TotalPrice;
            }

            _context.Orders.Add(order);

            // Səbəti boşaldırıq.
            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToDto(order);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // İstifadəçinin öz sifarişlərini qaytarır.
    public async Task<List<OrderDto>> GetMyOrdersAsync(int userId)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    // Admin üçün bütün müştərilərin sifarişlərini qaytarır.
    public async Task<List<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    // İstifadəçinin konkret sifarişini nömrəyə görə tapır.
    public async Task<OrderDto?> GetOrderByNumberAsync(
        int userId,
        string orderNumber)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.OrderNumber == orderNumber);

        if (order == null)
        {
            return null;
        }

        return MapToDto(order);
    }

    // Admin sifarişin statusunu dəyişir.
    public async Task<bool> UpdateOrderStatusAsync(
        string orderNumber,
        string status)
    {
        var allowedStatuses = new[]
        {
        "Pending",
        "Confirmed",
        "Preparing",
        "Shipped",
        "Delivered",
        "Cancelled"
    };

        if (!allowedStatuses.Contains(status))
        {
            return false;
        }

        var order = await _context.Orders
            .FirstOrDefaultAsync(x =>
                x.OrderNumber == orderNumber);

        if (order == null)
        {
            return false;
        }

        order.Status = status;

        await _context.SaveChangesAsync();

        return true;
    }

    // Order obyektini API üçün DTO-ya çevirir.
    private static OrderDto MapToDto(
        KENNArchive.Domain.Entities.Order order)
    {
        return new OrderDto
        {
            OrderNumber = order.OrderNumber,
            TotalPrice = order.TotalPrice,
            Status = order.Status,
            ShippingAddress = order.ShippingAddress,
            CreatedAt = order.CreatedAt,

            Items = order.Items.Select(item => new OrderItemDto
            {
                ProductName = item.ProductName,
                ProductSlug = item.Product?.Slug ?? string.Empty,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.TotalPrice
            }).ToList()
        };
    }

}
