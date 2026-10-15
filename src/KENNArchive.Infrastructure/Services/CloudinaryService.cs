using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using KENNArchive.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace KENNArchive.Infrastructure.Services;

// Şəkilləri Cloudinary-yə yükləyir və silir.
public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        var account = new Account(
            cloudName,
            apiKey,
            apiSecret);

        _cloudinary = new Cloudinary(account);
    }

    // Şəkli göstərilən Cloudinary qovluğuna yükləyir.
    // Qovluq verilməsə, əvvəlki kimi products istifadə olunur.
    public async Task<(string ImageUrl, string PublicId)> UploadImageAsync(
        Stream fileStream,
        string fileName,
        string folder = "kennarchive/products")
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            throw new Exception(
                $"Cloudinary upload xətası: {result.Error.Message}");
        }

        return (
            result.SecureUrl.ToString(),
            result.PublicId);
    }

    // Cloudinary-də olan şəkli silir.
    public async Task DeleteImageAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);

        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Error != null)
        {
            throw new Exception(
                $"Cloudinary delete xətası: {result.Error.Message}");
        }
    }
}