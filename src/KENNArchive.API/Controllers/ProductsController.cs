using KENNArchive.Application.DTOs.Product;
using KENNArchive.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KENNArchive.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IProductImageService _productImageService;
public ProductsController(
    IProductService productService,
    ICloudinaryService cloudinaryService,
    IProductImageService productImageService)
    {
        _productService = productService;
        _cloudinaryService = cloudinaryService;
        _productImageService = productImageService;
    }

    // Yeni məhsul yaradır.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request)
    {
        var product = await _productService.CreateAsync(request);

        if (product == null)
        {
            return BadRequest(
                "Category və ya Brand tapılmadı, yaxud bu adda Product artıq mövcuddur.");
        }

        return Ok(product);
    }

    // Məhsulları axtarış, brend, kateqoriya,
    // qiymət, sıralama və pagination ilə gətirir.
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] string? brand,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var products = await _productService.GetProductsAsync(
            search,
            category,
            brand,
            minPrice,
            maxPrice,
            sort,
            page,
            pageSize);

        return Ok(products);
    }

    // Slug-a görə məhsulun detallarını gətirir.
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var product = await _productService.GetBySlugAsync(slug);

        if (product == null)
        {
            return NotFound("Məhsul tapılmadı.");
        }

        return Ok(product);
    }

    // Məhsulun məlumatlarını yeniləyir.
    [HttpPut("{slug}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        string slug,
        [FromBody] UpdateProductRequest request)
    {
        var updated = await _productService.UpdateAsync(
            slug,
            request);

        if (!updated)
        {
            return BadRequest(
                "Məhsul, Category və ya Brand tapılmadı.");
        }

        return Ok("Məhsul uğurla yeniləndi.");
    }

    // Məhsulu silir.
    [HttpDelete("{slug}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string slug)
    {
        var deleted = await _productService.DeleteAsync(slug);

        if (!deleted)
        {
            return NotFound("Məhsul tapılmadı.");
        }

        return Ok("Məhsul uğurla silindi.");
    }

    // Məhsula aid şəkilləri gətirir.
    [HttpGet("{slug}/images")]
    public async Task<IActionResult> GetImages(string slug)
    {
        var productId = await _productService.GetIdBySlugAsync(slug);

        if (productId == null)
        {
            return NotFound("Məhsul tapılmadı.");
        }

        var images = await _productImageService
            .GetByProductIdAsync(productId.Value);

        return Ok(images);
    }

    // Məhsulun şəklini Cloudinary-yə yükləyir.
    [HttpPost("{slug}/images")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage(
        string slug,
        [FromForm] UploadProductImageRequest request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("Şəkil seçilməyib.");
        }

        var productId = await _productService.GetIdBySlugAsync(slug);

        if (productId == null)
        {
            return NotFound("Məhsul tapılmadı.");
        }

        await using var stream = request.File.OpenReadStream();

        var cloudinaryResult =
            await _cloudinaryService.UploadImageAsync(
                stream,
                request.File.FileName);

        var saved = await _productImageService.AddAsync(
            productId.Value,
            cloudinaryResult.ImageUrl,
            cloudinaryResult.PublicId);

        if (!saved)
        {
            return BadRequest(
                "Şəkil database-ə yazıla bilmədi.");
        }

        return Ok(new
        {
            Message = "Şəkil uğurla yükləndi.",
            ImageUrl = cloudinaryResult.ImageUrl,
            PublicId = cloudinaryResult.PublicId
        });
    }

    // Şəkli Cloudinary və database-dən silir.
    [HttpDelete("images/{imageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteImage(int imageId)
    {
        var deleted = await _productImageService.DeleteAsync(imageId);

        if (!deleted)
        {
            return NotFound("Şəkil tapılmadı.");
        }

        return Ok("Şəkil uğurla silindi.");
    }

}
