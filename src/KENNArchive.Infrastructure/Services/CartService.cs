using KENNArchive.Application.DTOs.Cart;
using KENNArchive.Application.Interfaces;
using KENNArchive.Domain.Entities;
using KENNArchive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KENNArchive.Infrastructure.Services;

// İstifadəçinin səbət əməliyyatlarını database üzərindən idarə edir.
public class CartService : ICartService
{
    private readonly KennArchiveDbContext _context;

    public CartService(KennArchiveDbContext context)
    {
        _context = context;
    }

    // İstifadəçinin səbətini gətirir.
    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await _context.Carts
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null)
        {
            return new CartDto();
        }

        var items = cart.Items
            .Where(x => x.Product != null)
            .Select(x => new CartItemDto
            {
                ProductName = x.Product!.Name,
                ProductSlug = x.Product.Slug,
                Price = x.Product.Price,
                Quantity = x.Quantity,
                TotalPrice = x.Product.Price * x.Quantity
            })
            .ToList();

        return new CartDto
        {
            Items = items,
            TotalPrice = items.Sum(x => x.TotalPrice)
        };
    }

    // Səbətə məhsul əlavə edir.
    public async Task<bool> AddItemAsync(
        int userId,
        AddCartItemRequest request)
    {
        if (request.Quantity <= 0)
        {
            return false;
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Slug == request.ProductSlug);

        if (product == null)
        {
            return false;
        }

        if (product.Stock < request.Quantity)
        {
            return false;
        }

        var cart = await _context.Carts
            .FirstOrDefaultAsync(x =>
                x.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId
            };

            _context.Carts.Add(cart);

            await _context.SaveChangesAsync();
        }

        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(x =>
                x.CartId == cart.Id &&
                x.ProductId == product.Id);

        if (existingItem != null)
        {
            var newQuantity =
                existingItem.Quantity + request.Quantity;

            if (newQuantity > product.Stock)
            {
                return false;
            }

            existingItem.Quantity = newQuantity;
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = request.Quantity
            };

            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync();

        return true;
    }

    // Səbətdəki məhsulun sayını dəyişir.
    public async Task<bool> UpdateItemAsync(
        int userId,
        string productSlug,
        int quantity)
    {
        if (quantity <= 0)
        {
            return false;
        }

        var item = await _context.CartItems
            .Include(x => x.Cart)
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.Cart != null &&
                x.Cart.UserId == userId &&
                x.Product != null &&
                x.Product.Slug == productSlug);

        if (item == null || item.Product == null)
        {
            return false;
        }

        if (quantity > item.Product.Stock)
        {
            return false;
        }

        item.Quantity = quantity;

        await _context.SaveChangesAsync();

        return true;
    }

    // Məhsulu səbətdən silir.
    public async Task<bool> RemoveItemAsync(
        int userId,
        string productSlug)
    {
        var item = await _context.CartItems
            .Include(x => x.Cart)
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.Cart != null &&
                x.Cart.UserId == userId &&
                x.Product != null &&
                x.Product.Slug == productSlug);

        if (item == null)
        {
            return false;
        }

        _context.CartItems.Remove(item);

        await _context.SaveChangesAsync();

        return true;
    }
}