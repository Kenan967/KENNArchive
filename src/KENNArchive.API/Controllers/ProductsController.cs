using KENNArchive.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KENNArchive.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // Məhsul siyahısı search, filter, sorting və pagination parametrlərini URL-dən qəbul edir.
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var products = await _productService.GetProductsAsync(
            search, category, minPrice, maxPrice, sort, page, pageSize);

        return Ok(products);
    }
}
