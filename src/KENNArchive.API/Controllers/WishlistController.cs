using System.Security.Claims;
using KENNArchive.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KENNArchive.API.Controllers;

[ApiController]
[Route("api/wishlist")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    // Login olmuş istifadəçinin Wishlist-ini gətirir.
    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var wishlist = await _wishlistService
            .GetWishlistAsync(userId.Value);

        return Ok(wishlist);
    }

    // Məhsulu Wishlist-ə əlavə edir.
    [HttpPost("items/{productSlug}")]
    public async Task<IActionResult> Add(
        string productSlug)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var added = await _wishlistService.AddAsync(
            userId.Value,
            productSlug);

        if (!added)
        {
            return BadRequest(
                "Məhsul tapılmadı və ya artıq Wishlist-dədir.");
        }

        return Ok("Məhsul Wishlist-ə əlavə edildi.");
    }

    // Məhsulu Wishlist-dən silir.
    [HttpDelete("items/{productSlug}")]
    public async Task<IActionResult> Remove(
        string productSlug)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var removed = await _wishlistService.RemoveAsync(
            userId.Value,
            productSlug);

        if (!removed)
        {
            return NotFound(
                "Məhsul Wishlist-də tapılmadı.");
        }

        return Ok("Məhsul Wishlist-dən silindi.");
    }

    // JWT token-dan login olmuş istifadəçinin ID-sini götürür.
    private int? GetUserId()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (int.TryParse(userIdValue, out var userId))
        {
            return userId;
        }

        return null;
    }
}