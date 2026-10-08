using KENNArchive.Application.DTOs.Product;

namespace KENNArchive.Application.Interfaces;

// Product şəkillərinin database əməliyyatlarını idarə edir.
public interface IProductImageService
{
    Task<bool> AddAsync(
        int productId,
        string imageUrl,
        string publicId);
    Task<bool> DeleteAsync(
        int imageId);
    Task<IReadOnlyList<ProductImageDto>> GetByProductIdAsync(
        int productId);
}