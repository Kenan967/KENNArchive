namespace KENNArchive.Application.Interfaces;
// Cloudinary ilə şəkil yükləmə və silmə əməliyyatlarını idarə edir.
public interface ICloudinaryService
{
    Task<(string ImageUrl, string PublicId)> UploadImageAsync(
        Stream fileStream,
        string fileName);
    Task DeleteImageAsync(string publicId);
}