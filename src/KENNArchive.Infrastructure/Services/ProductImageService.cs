using KENNArchive.Application.DTOs.Product;
using KENNArchive.Application.Interfaces;
using KENNArchive.Domain.Entities;
using KENNArchive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KENNArchive.Infrastructure.Services;

// Product şəkillərinin database və Cloudinary əməliyyatlarını idarə edir.
public class ProductImageService : IProductImageService
{
    private readonly KennArchiveDbContext _context;
    private readonly ICloudinaryService _cloudinaryService;

    public ProductImageService(
        KennArchiveDbContext context,
        ICloudinaryService cloudinaryService)
    {
        _context = context;
        _cloudinaryService = cloudinaryService;
    }

    // Product şəklini database-ə əlavə edir.
    public async Task<bool> AddAsync(
        int productId,
        string imageUrl,
        string publicId)
    {
        var productExists = await _context.Products
            .AnyAsync(x => x.Id == productId);

        if (!productExists)
        {
            return false;
        }

        var productImage = new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl,
            PublicId = publicId
        };

        _context.ProductImages.Add(productImage);

        await _context.SaveChangesAsync();

        return true;
    }

    // Product-a aid şəkilləri gətirir.
    public async Task<IReadOnlyList<ProductImageDto>> GetByProductIdAsync(
        int productId)
    {
        return await _context.ProductImages
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .Select(x => new ProductImageDto
            {
                Id = x.Id,
                ImageUrl = x.ImageUrl,
                PublicId = x.PublicId
            })
            .ToListAsync();
    }

    // Product şəklini həm Cloudinary-dən,
    // həm də database-dən silir.
    public async Task<bool> DeleteAsync(int imageId)
    {
        var image = await _context.ProductImages
            .FirstOrDefaultAsync(x => x.Id == imageId);

        if (image == null)
        {
            return false;
        }

        // Şəkli Cloudinary-dən silirik.
        await _cloudinaryService.DeleteImageAsync(
            image.PublicId);

        // Database-dəki məlumatı silirik.
        _context.ProductImages.Remove(image);

        await _context.SaveChangesAsync();

        return true;
    }
}