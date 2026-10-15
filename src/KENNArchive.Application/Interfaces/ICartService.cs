using KENNArchive.Application.DTOs.Cart;

namespace KENNArchive.Application.Interfaces;

// Səbət əməliyyatlarının müqaviləsi.
public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);

    Task<bool> AddItemAsync(
        int userId,
        AddCartItemRequest request);

    Task<bool> UpdateItemAsync(
        int userId,
        string productSlug,
        int productVariantId,
        int quantity);

    Task<bool> RemoveItemAsync(
        int userId,
        string productSlug,
        int productVariantId);
}