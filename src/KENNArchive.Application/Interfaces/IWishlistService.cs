using KENNArchive.Application.DTOs.Wishlist;
namespace KENNArchive.Application.Interfaces;

// Wishlist əməliyyatlarını idarə edir.
public interface IWishlistService
{
    Task<WishlistDto> GetWishlistAsync(int userId);
    Task<bool> AddAsync(
        int userId,
        string productSlug);
    Task<bool> RemoveAsync(
        int userId,
        string productSlug);
}