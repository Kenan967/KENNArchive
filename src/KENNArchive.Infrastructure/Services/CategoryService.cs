using KENNArchive.Application.DTOs.Category;
using KENNArchive.Application.Interfaces;
using KENNArchive.Domain.Entities;
using KENNArchive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace KENNArchive.Infrastructure.Services;
public class CategoryService : ICategoryService
{


    private readonly KennArchiveDbContext _dbContext;

    public CategoryService(KennArchiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Category> CreateAsync(
        CreateCategoryRequest request)
    {
        var slug = request.Name
            .Trim()
            .ToLower()
            .Replace(" ", "-");

        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = slug
        };

        _dbContext.Categories.Add(category);

        await _dbContext.SaveChangesAsync();

        return category;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _dbContext.Categories
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetBySlugAsync(string slug)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(x => x.Slug == slug);
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category == null)
        {
            return false;
        }
        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}