using KENNArchive.Application.DTOs.Wishlist;
using KENNArchive.Application.Interfaces;
using KENNArchive.Domain.Entities;
using KENNArchive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KENNArchive.Infrastructure.Services;

// İstifadəçinin Wishlist əməliyyatlarını database üzərindən idarə edir.
public class WishlistService : IWishlistService
{
    private readonly KennArchiveDbContext _context;

    public WishlistService(KennArchiveDbContext context)
    {
        _context = context;
    }

    // İstifadəçinin Wishlist-ini gətirir.
    public async Task<WishlistDto> GetWishlistAsync(int userId)
    {
        var wishlist = await _context.Wishlists
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (wishlist == null)
        {
            return new WishlistDto();
        }

        var items = wishlist.Items
            .Where(x => x.Product != null)
            .Select(x => new WishlistItemDto
            {
                ProductName = x.Product!.Name,
                ProductSlug = x.Product.Slug,
                Price = x.Product.Price
            })
            .ToList();

        return new WishlistDto
        {
            Items = items
        };
    }

    // Məhsulu Wishlist-ə əlavə edir.
    public async Task<bool> AddAsync(
        int userId,
        string productSlug)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Slug == productSlug);

        if (product == null)
        {
            return false;
        }

        var wishlist = await _context.Wishlists
            .FirstOrDefaultAsync(x =>
                x.UserId == userId);

        if (wishlist == null)
        {
            wishlist = new Wishlist
            {
                UserId = userId
            };

            _context.Wishlists.Add(wishlist);

            await _context.SaveChangesAsync();
        }

        var alreadyExists = await _context.WishlistItems
            .AnyAsync(x =>
                x.WishlistId == wishlist.Id &&
                x.ProductId == product.Id);

        if (alreadyExists)
        {
            return false;
        }

        var wishlistItem = new WishlistItem
        {
            WishlistId = wishlist.Id,
            ProductId = product.Id
        };

        _context.WishlistItems.Add(wishlistItem);

        await _context.SaveChangesAsync();

        return true;
    }

    // Məhsulu Wishlist-dən silir.
    public async Task<bool> RemoveAsync(
        int userId,
        string productSlug)
    {
        var item = await _context.WishlistItems
            .Include(x => x.Wishlist)
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.Wishlist != null &&
                x.Wishlist.UserId == userId &&
                x.Product != null &&
                x.Product.Slug == productSlug);

        if (item == null)
        {
            return false;
        }

        _context.WishlistItems.Remove(item);

        await _context.SaveChangesAsync();

        return true;
    }
}