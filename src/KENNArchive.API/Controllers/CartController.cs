using System.Security.Claims;
using KENNArchive.Application.DTOs.Cart;
using KENNArchive.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KENNArchive.API.Controllers;



[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    // Hazırda login olmuş istifadəçinin səbətini gətirir.
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var cart = await _cartService.GetCartAsync(userId.Value);

        return Ok(cart);
    }

    // Səbətə məhsul əlavə edir.
    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        AddCartItemRequest request)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var added = await _cartService.AddItemAsync(
            userId.Value,
            request);

        if (!added)
        {
            return BadRequest(
                "Məhsul tapılmadı və ya kifayət qədər stok yoxdur.");
        }

        return Ok("Məhsul səbətə əlavə edildi.");
    }

    // Səbətdəki məhsulun sayını dəyişir.
    [HttpPut("items/{productSlug}")]
    public async Task<IActionResult> UpdateItem(
        string productSlug,
        int quantity)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var updated = await _cartService.UpdateItemAsync(
            userId.Value,
            productSlug,
            quantity);

        if (!updated)
        {
            return BadRequest(
                "Məhsul tapılmadı və ya stok kifayət deyil.");
        }

        return Ok("Səbət yeniləndi.");
    }

    // Məhsulu səbətdən silir.
    [HttpDelete("items/{productSlug}")]
    public async Task<IActionResult> RemoveItem(
        string productSlug)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var removed = await _cartService.RemoveItemAsync(
            userId.Value,
            productSlug);

        if (!removed)
        {
            return NotFound("Məhsul səbətdə tapılmadı.");
        }

        return Ok("Məhsul səbətdən silindi.");
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