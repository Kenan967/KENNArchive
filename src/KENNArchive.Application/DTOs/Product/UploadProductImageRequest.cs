using Microsoft.AspNetCore.Http;

namespace KENNArchive.Application.DTOs.Product;

// Product üçün yüklənəcək şəkili qəbul edir.
public class UploadProductImageRequest
{
    public IFormFile File { get; set; } = null!;
}