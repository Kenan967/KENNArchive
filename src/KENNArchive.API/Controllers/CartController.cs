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

    // Hazırkı istifadəçinin səbətini gətirir.
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

    // Seçilmiş ölçüdən məhsulu səbətə əlavə edir.
    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddCartItemRequest request)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var success = await _cartService.AddItemAsync(
            userId.Value,
            request);

        if (!success)
        {
            return BadRequest(new
            {
                message = "Məhsul və ya ölçü tapılmadı, yaxud həmin ölçüdə kifayət qədər stok yoxdur."
            });
        }

        return Ok(new
        {
            message = "Məhsul seçilmiş ölçü ilə səbətə əlavə edildi."
        });
    }

    // Seçilmiş ölçünün sayını dəyişir.
    // quantity query parametrindən göndərilir.
    [HttpPut("items/{productSlug}/{productVariantId:int}")]
    public async Task<IActionResult> UpdateItem(
        string productSlug,
        int productVariantId,
        [FromQuery] int quantity)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var success = await _cartService.UpdateItemAsync(
            userId.Value,
            productSlug,
            productVariantId,
            quantity);

        if (!success)
        {
            return BadRequest(new
            {
                message = "Səbət yenilənmədi. Məhsulu, ölçünü və stoku yoxlayın."
            });
        }

        return Ok(new
        {
            message = "Səbət yeniləndi."
        });
    }

    // Səbətdən yalnız seçilmiş ölçünü silir.
    [HttpDelete("items/{productSlug}/{productVariantId:int}")]
    public async Task<IActionResult> RemoveItem(
        string productSlug,
        int productVariantId)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var success = await _cartService.RemoveItemAsync(
            userId.Value,
            productSlug,
            productVariantId);

        if (!success)
        {
            return NotFound(new
            {
                message = "Məhsulun bu ölçüsü səbətdə tapılmadı."
            });
        }

        return Ok(new
        {
            message = "Məhsul səbətdən silindi."
        });
    }

    // JWT tokenindən istifadəçi ID-sini götürür.
    private int? GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (int.TryParse(userId, out var id))
        {
            return id;
        }

        return null;
    }
}