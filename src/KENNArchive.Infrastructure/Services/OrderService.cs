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
    private readonly IEmailService _emailService;

    public OrderService(
        KennArchiveDbContext context,
        IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // Səbətdəki məhsullardan sifariş yaradır.
    public async Task<OrderDto?> CreateOrderAsync(
        int userId,
        CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
            return null;

        var cart = await _context.Carts
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .Include(x => x.Items)
                .ThenInclude(x => x.ProductVariant)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null || !cart.Items.Any())
            return null;

        foreach (var item in cart.Items)
        {
            if (item.Product == null ||
                item.ProductVariant == null ||
                item.ProductVariantId == null ||
                item.Quantity <= 0)
            {
                return null;
            }

            if (item.ProductVariant.ProductId != item.Product.Id)
                return null;

            if (item.ProductVariant.Stock < item.Quantity)
                return null;
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var order = new Order
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
                var variant = cartItem.ProductVariant!;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Size = variant.Size,
                    UnitPrice = product.Price,
                    Quantity = cartItem.Quantity,
                    TotalPrice = product.Price * cartItem.Quantity
                };

                order.Items.Add(orderItem);

                // Seçilmiş ölçünün stokunu azaldırıq.
                variant.Stock -= cartItem.Quantity;

                // Ümumi stoku da uyğun saxlayırıq.
                product.Stock = Math.Max(
                    0,
                    product.Stock - cartItem.Quantity);

                order.TotalPrice += orderItem.TotalPrice;
            }

            _context.Orders.Add(order);
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

    // İstifadəçinin öz sifarişlərini gətirir.
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

    // Bütün sifarişləri gətirir.
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

    // İstifadəçinin sifarişini nömrə ilə tapır.
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

        return order == null ? null : MapToDto(order);
    }

    // Sifariş statusunu dəyişir və ləğv zamanı stoku bərpa edir.
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

        if (string.IsNullOrWhiteSpace(status) ||
            !allowedStatuses.Contains(status))
        {
            return false;
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        Order? order;

        try
        {
            order = await _context.Orders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x =>
                    x.OrderNumber == orderNumber);

            if (order == null)
                return false;

            // Eyni statusdursa, heç nəyi təkrar etmirik.
            if (order.Status == status)
                return true;

            // Ləğv olunmuş sifariş yenidən aktivləşdirilmir.
            // Beləliklə, stokun ikinci dəfə bərpasının qarşısı alınır.
            if (order.Status == "Cancelled")
                return false;

            if (status == "Cancelled")
            {
                foreach (var item in order.Items)
                {
                    // Köhnə sifarişlərdə ölçü boş ola bilər.
                    // Onların stokunu səhvən artırmırıq.
                    if (string.IsNullOrWhiteSpace(item.Size))
                        continue;

                    var variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(x =>
                            x.ProductId == item.ProductId &&
                            x.Size == item.Size);

                    // Ölçü variantı tapılmırsa, stok bərpasını
                    // səssizcə ötürmək əvəzinə əməliyyatı dayandırırıq.
                    if (variant == null)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    // Sifariş zamanı azaldılmış ölçü stokunu bərpa et.
                    variant.Stock += item.Quantity;

                    // Köhnə ümumi stok sahəsini də bərpa et.
                    var product = await _context.Products
                        .FirstOrDefaultAsync(x =>
                            x.Id == item.ProductId);

                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                    }
                }
            }

            order.Status = status;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        // Email xətası status dəyişikliyini geri qaytarmamalıdır.
        var customerEmail = await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == order.UserId)
            .Select(x => x.Email)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            Console.WriteLine(
                $"Sifariş {order.OrderNumber} üçün müştəri emaili tapılmadı.");

            return true;
        }

        try
        {
            await _emailService.SendOrderStatusUpdateAsync(
                customerEmail,
                order.OrderNumber,
                order.Status);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Sifariş emaili göndərilmədi: {ex.Message}");
        }

        return true;
    }

    // Sifarişi DTO-ya çevirir.
    private static OrderDto MapToDto(Order order)
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
                Size = item.Size,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.TotalPrice
            }).ToList()
        };
    }
}
