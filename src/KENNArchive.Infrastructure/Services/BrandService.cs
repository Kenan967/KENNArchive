using KENNArchive.Application.DTOs.Brand;
using KENNArchive.Application.Interfaces;
using KENNArchive.Domain.Entities;
using KENNArchive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KENNArchive.Infrastructure.Services;
public class BrandService : IBrandService
{
    private readonly KennArchiveDbContext _dbContext;
    public BrandService(KennArchiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Brand> CreateAsync(
        CreateBrandRequest request)
    {
        var slug = request.Name
            .Trim()
            .ToLower()
            .Replace(" ", "-");

        var brand = new Brand
        {
            Name = request.Name.Trim(),
            Slug = slug
        };
        _dbContext.Brands.Add(brand);
        await _dbContext.SaveChangesAsync();
        return brand;
    }
    public async Task<List<Brand>> GetAllAsync()
    {
        return await _dbContext.Brands
            .OrderBy(x => x.Name)
            .ToListAsync();
    }
    public async Task<Brand?> GetBySlugAsync(string slug)
    {
        return await _dbContext.Brands
            .FirstOrDefaultAsync(x => x.Slug == slug);
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var brand = await _dbContext.Brands
            .FirstOrDefaultAsync(x => x.Id == id);
        if (brand == null)
        {
            return false;
        }
        _dbContext.Brands.Remove(brand);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}