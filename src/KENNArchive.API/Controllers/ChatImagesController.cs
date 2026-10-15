using KENNArchive.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KENNArchive.API.Controllers;

// Chat şəkillərini Cloudinary-yə yükləyir.
[ApiController]
[Route("api/chat/images")]
[Authorize]
public class ChatImagesController : ControllerBase
{
    private readonly ICloudinaryService _cloudinaryService;

public ChatImagesController(ICloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    // Şəkli Cloudinary-yə yükləyir və URL qaytarır.
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "Zəhmət olmasa, şəkil seçin."
            });
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new
            {
                message = "Şəklin ölçüsü maksimum 5 MB ola bilər."
            });
        }

        var allowedTypes = new[]
        {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new
            {
                message = "Yalnız JPG, PNG, WEBP və GIF şəkilləri qəbul edilir."
            });
        }

        try
        {
            await using var stream = file.OpenReadStream();

            var result = await _cloudinaryService.UploadImageAsync(
                stream,
                Path.GetFileName(file.FileName),
                "kennarchive/chat"
            );

            return Ok(new
            {
                imageUrl = result.ImageUrl,
                publicId = result.PublicId
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                message = "Şəkli yükləmək mümkün olmadı."
            });
        }
    }

}
