using KENNArchive.Application.DTOs.Brand;
using KENNArchive.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace KENNArchive.API.Controllers;



[ApiController]
[Route("api/[controller]")]
public class BrandController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreateBrandRequest request)
    {
        var brand = await _brandService.CreateAsync(request);

        return Ok(brand);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var brands = await _brandService.GetAllAsync();



        return Ok(brands);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var brand = await _brandService.GetBySlugAsync(slug);

        if (brand == null)
        {
            return NotFound("Marka tapılmadı.");
        }

        return Ok(brand);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _brandService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound("Marka tapılmadı.");
        }


        return Ok("Marka silindi.");
    }
}