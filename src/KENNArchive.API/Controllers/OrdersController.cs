using System.Security.Claims;
using KENNArchive.Application.DTOs.Order;
using KENNArchive.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KENNArchive.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // Müştəri səbətindən yeni sifariş yaradır.
    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        CreateOrderRequest request)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var order = await _orderService.CreateOrderAsync(
            userId.Value,
            request);

        if (order == null)
        {
            return BadRequest(
                "Sifariş yaradıla bilmədi. Səbəti və məhsul stokunu yoxlayın.");
        }

        return Ok(order);
    }

    // İstifadəçinin yalnız öz sifarişlərini göstərir.
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var orders = await _orderService.GetMyOrdersAsync(
            userId.Value);

        return Ok(orders);
    }

    // Bütün sifarişləri yalnız Admin görə bilər.
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();

        return Ok(orders);
    }

    // İstifadəçinin konkret sifarişini nömrəyə görə göstərir.
    [HttpGet("{orderNumber}")]
    public async Task<IActionResult> GetOrder(
        string orderNumber)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var order = await _orderService.GetOrderByNumberAsync(
            userId.Value,
            orderNumber);

        if (order == null)
        {
            return NotFound("Sifariş tapılmadı.");
        }

        return Ok(order);
    }

    // Yalnız Admin sifariş statusunu dəyişə bilər.
    [HttpPut("{orderNumber}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(
        string orderNumber,
        [FromQuery] string status)
    {
        var updated = await _orderService.UpdateOrderStatusAsync(
            orderNumber,
            status);

        if (!updated)
        {
            return BadRequest(
                "Sifariş tapılmadı və ya status yanlışdır.");
        }

        return Ok("Sifariş statusu yeniləndi.");
    }

    // İstifadəçinin ID-sini JWT tokenindən götürür.
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
