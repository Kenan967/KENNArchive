using KENNArchive.Application.DTOs.Cart;
using KENNArchive.Application.Interfaces;
using KENNArchive.Domain.Entities;
using KENNArchive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KENNArchive.Infrastructure.Services;

// İstifadəçinin səbət əməliyyatlarını idarə edir.
public class CartService : ICartService
{
    private readonly KennArchiveDbContext _context;

    public CartService(KennArchiveDbContext context)
    {
        _context = context;
    }

    // İstifadəçinin səbətini ölçülərlə birlikdə gətirir.
    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await _context.Carts
            .AsNoTracking()
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .Include(x => x.Items)
                .ThenInclude(x => x.ProductVariant)
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

                ProductVariantId = x.ProductVariantId ?? 0,

                Size = x.ProductVariant?.Size ?? string.Empty,

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

    // Seçilmiş ölçüdən məhsulu səbətə əlavə edir.
    public async Task<bool> AddItemAsync(
        int userId,
        AddCartItemRequest request)
    {
        if (request.Quantity <= 0 ||
            string.IsNullOrWhiteSpace(request.ProductSlug) ||
            request.ProductVariantId <= 0)
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

        var variant = await _context.ProductVariants
            .FirstOrDefaultAsync(x =>
                x.Id == request.ProductVariantId &&
                x.ProductId == product.Id);

        if (variant == null)
        {
            return false;
        }

        if (variant.Stock < request.Quantity)
        {
            return false;
        }

        var cart = await _context.Carts
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId
            };

            _context.Carts.Add(cart);

            await _context.SaveChangesAsync();
        }

        // Eyni məhsulun eyni ölçüsü bir sətirdə saxlanılır.
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(x =>
                x.CartId == cart.Id &&
                x.ProductId == product.Id &&
                x.ProductVariantId == variant.Id);

        if (existingItem != null)
        {
            var newQuantity =
                existingItem.Quantity + request.Quantity;

            if (newQuantity > variant.Stock)
            {
                return false;
            }

            existingItem.Quantity = newQuantity;
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                ProductVariantId = variant.Id,
                Quantity = request.Quantity
            });
        }

        await _context.SaveChangesAsync();

        return true;
    }

    // Səbətdə seçilmiş ölçünün sayını dəyişir.
    public async Task<bool> UpdateItemAsync(
        int userId,
        string productSlug,
        int productVariantId,
        int quantity)
    {
        if (quantity <= 0 || productVariantId <= 0)
        {
            return false;
        }

        var item = await _context.CartItems
            .Include(x => x.Cart)
            .Include(x => x.Product)
            .Include(x => x.ProductVariant)
            .FirstOrDefaultAsync(x =>
                x.Cart != null &&
                x.Cart.UserId == userId &&
                x.Product != null &&
                x.Product.Slug == productSlug &&
                x.ProductVariantId == productVariantId);

        if (item == null || item.ProductVariant == null)
        {
            return false;
        }

        if (quantity > item.ProductVariant.Stock)
        {
            return false;
        }

        item.Quantity = quantity;

        await _context.SaveChangesAsync();

        return true;
    }

    // Səbətdən yalnız seçilmiş ölçünü silir.
    public async Task<bool> RemoveItemAsync(
        int userId,
        string productSlug,
        int productVariantId)
    {
        if (productVariantId <= 0)
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
                x.Product.Slug == productSlug &&
                x.ProductVariantId == productVariantId);

        if (item == null)
        {
            return false;
        }

        _context.CartItems.Remove(item);

        await _context.SaveChangesAsync();

        return true;
    }
}