using KENNArchive.Application.DTOs.Category;
using KENNArchive.Domain.Entities;

namespace KENNArchive.Application.Interfaces;

public interface ICategoryService
{
    Task<Category> CreateAsync(CreateCategoryRequest request);

    Task<List<Category>> GetAllAsync();

    Task<Category?> GetBySlugAsync(string slug);

    Task<bool> DeleteAsync(int id);
}