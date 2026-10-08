using KENNArchive.Application.DTOs.Cart;

namespace KENNArchive.Application.Interfaces;

// İstifadəçi səbətinin əməliyyatlarını idarə edir.
public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);

    Task<bool> AddItemAsync(
        int userId,
        AddCartItemRequest request);

    Task<bool> UpdateItemAsync(
        int userId,
        string productSlug,
        int quantity);

    Task<bool> RemoveItemAsync(
        int userId,
        string productSlug);
}