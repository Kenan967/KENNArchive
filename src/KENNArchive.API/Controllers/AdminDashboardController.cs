using KENNArchive.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KENNArchive.API.Controllers;

// Admin paneli üçün ümumi statistik məlumatlar.
[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly KennArchiveDbContext _context;

    public AdminDashboardController(
        KennArchiveDbContext context)
    {
        _context = context;
    }

    // Admin panelinin əsas statistikalarını qaytarır.
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var totalProducts = await _context.Products.CountAsync();

        var totalCategories = await _context.Categories.CountAsync();

        var totalBrands = await _context.Brands.CountAsync();

        var totalOrders = await _context.Orders.CountAsync();

        var totalUsers = await _context.Users.CountAsync();

        var pendingReturns = await _context.ReturnRequests
            .CountAsync(x => x.Status == "Pending");

        var deliveredOrders = await _context.Orders
            .CountAsync(x => x.Status == "Delivered");

        var totalRevenue = await _context.Orders
            .Where(x => x.Status == "Delivered")
            .SumAsync(x => (decimal?)x.TotalPrice) ?? 0m;

        var lowStockProducts = await _context.Products
            .CountAsync(x => x.Stock <= 5);

        return Ok(new
        {
            totalProducts,
            totalCategories,
            totalBrands,
            totalOrders,
            totalUsers,
            pendingReturns,
            deliveredOrders,
            totalRevenue,
            lowStockProducts
        });
    }
}