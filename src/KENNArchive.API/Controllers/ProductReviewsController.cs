using System.Security.Claims;
using KENNArchive.Domain.Entities;
using KENNArchive.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KENNArchive.API.Controllers;

[ApiController]
[Route("api/products/{slug}/reviews")]
public class ProductReviewsController : ControllerBase
{
    private readonly KennArchiveDbContext _context;

    public ProductReviewsController(KennArchiveDbContext context)
    {
        _context = context;
    }

    // Məhsulun bütün rəylərini gətirir.
    [HttpGet]
    public async Task<IActionResult> GetReviews(string slug)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (product == null)
        {
            return NotFound("Məhsul tapılmadı.");
        }

        var reviews = await _context.ProductReviews
            .AsNoTracking()
            .Where(r => r.ProductId == product.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                UserName = r.User == null
                    ? "İstifadəçi"
                    : (r.User.FirstName + " " + r.User.LastName).Trim()
            })
            .ToListAsync();

        var averageRating = reviews.Count == 0
            ? 0
            : Math.Round(reviews.Average(r => r.Rating), 1);

        return Ok(new
        {
            AverageRating = averageRating,
            TotalReviews = reviews.Count,
            Reviews = reviews
        });
    }

    // Daxil olmuş istifadəçi yeni rəy yazır.
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateReview(
        string slug,
        [FromBody] CreateProductReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            return BadRequest("Qiymətləndirmə 1 ilə 5 arasında olmalıdır.");
        }

        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            return BadRequest("Rəy mətni boş ola bilməz.");
        }

        if (request.Comment.Trim().Length > 1000)
        {
            return BadRequest("Rəy maksimum 1000 simvol ola bilər.");
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (product == null)
        {
            return NotFound("Məhsul tapılmadı.");
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized("İstifadəçi məlumatı tapılmadı.");
        }

        var userExists = await _context.Users
            .AnyAsync(u => u.Id == userId);

        if (!userExists)
        {
            return Unauthorized("İstifadəçi tapılmadı.");
        }

        var review = new ProductReview
        {
            ProductId = product.Id,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductReviews.Add(review);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Rəyiniz uğurla əlavə edildi.",
            review.Id,
            review.Rating,
            review.Comment,
            review.CreatedAt
        });
    }
}

public class CreateProductReviewRequest
{
    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;
}