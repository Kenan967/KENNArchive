using KENNArchive.Application.DTOs.Brand;
using KENNArchive.Domain.Entities;

namespace KENNArchive.Application.Interfaces;
public interface IBrandService
{
    Task<Brand> CreateAsync(CreateBrandRequest request);
    Task<List<Brand>> GetAllAsync();
    Task<Brand?> GetBySlugAsync(string slug);
    Task<bool> DeleteAsync(int id);
}