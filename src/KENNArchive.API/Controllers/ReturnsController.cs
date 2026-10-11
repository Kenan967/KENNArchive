using System.Security.Claims;
using KENNArchive.Application.DTOs.Return;
using KENNArchive.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KENNArchive.API.Controllers;

// Qaytarılma müraciətlərini idarə edir.
[ApiController]
[Route("api/returns")]
[Authorize]
public class ReturnsController : ControllerBase
{
    private readonly IReturnService _returnService;

    public ReturnsController(IReturnService returnService)
    {
        _returnService = returnService;
    }

    // Müştəri yeni qaytarılma müraciəti yaradır.
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateReturnRequest request)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _returnService.CreateAsync(
            userId.Value,
            request);

        if (result == null)
        {
            return BadRequest(
                "Müraciət yaradıla bilmədi. " +
                "Sifarişin çatdırıldığını və məlumatları yoxlayın.");
        }

        return Ok(result);
    }

    // Müştəri yalnız öz qaytarılma müraciətlərini görür.
    [HttpGet("my")]
    public async Task<IActionResult> GetMyRequests()
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var requests = await _returnService.GetMyRequestsAsync(
            userId.Value);

        return Ok(requests);
    }

    // Admin bütün müştərilərin qaytarılma müraciətlərini görür.
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllRequests()
    {
        var requests = await _returnService.GetAllRequestsAsync();

        return Ok(requests);
    }

    // Admin müraciəti təsdiqləyir və ya rədd edir.
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromQuery] string status,
        [FromQuery] string adminNote = "")
    {
        var updated = await _returnService.UpdateStatusAsync(
            id,
            status,
            adminNote);

        if (!updated)
        {
            return BadRequest(
                "Müraciət tapılmadı, artıq işlənib və ya status yanlışdır.");
        }

        return Ok(
            "Qaytarılma müraciətinin statusu yeniləndi.");
    }

    // JWT token-dən istifadəçinin ID-sini götürür.
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
