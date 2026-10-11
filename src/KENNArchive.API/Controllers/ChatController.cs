using System.Security.Claims;
using KENNArchive.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KENNArchive.API.Controllers;

// İstifadəçilərin öz mesaj tarixçələrini əldə etməsini təmin edir.
[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly KennArchiveDbContext _context;

    public ChatController(KennArchiveDbContext context)
    {
        _context = context;
    }

    // İstifadəçinin başqa bir istifadəçi ilə mesaj tarixçəsini göstərir.
    [HttpGet("{otherUserId:int}/messages")]
    public async Task<IActionResult> GetMessages(int otherUserId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        if (otherUserId <= 0 || otherUserId == userId)
        {
            return BadRequest("İstifadəçi ID-si yanlışdır.");
        }

        var otherUserExists = await _context.Users
            .AnyAsync(x => x.Id == otherUserId);

        if (!otherUserExists)
        {
            return NotFound("İstifadəçi tapılmadı.");
        }

        var messages = await _context.ChatMessages
            .AsNoTracking()
            .Where(x =>
                (x.SenderUserId == userId &&
                 x.ReceiverUserId == otherUserId) ||
                (x.SenderUserId == otherUserId &&
                 x.ReceiverUserId == userId))
            .OrderBy(x => x.SentAt)
            .Select(x => new
            {
                x.Id,
                x.SenderUserId,
                x.ReceiverUserId,
                x.Message,
                x.IsRead,
                x.SentAt
            })
            .ToListAsync();

        return Ok(messages);
    }

    // Digər istifadəçidən gələn mesajları oxunmuş kimi işarələyir.
    [HttpPut("{otherUserId:int}/read")]
    public async Task<IActionResult> MarkMessagesAsRead(int otherUserId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        if (otherUserId <= 0 || otherUserId == userId)
        {
            return BadRequest("İstifadəçi ID-si yanlışdır.");
        }

        var otherUserExists = await _context.Users
            .AnyAsync(x => x.Id == otherUserId);

        if (!otherUserExists)
        {
            return NotFound("İstifadəçi tapılmadı.");
        }

        var messages = await _context.ChatMessages
            .Where(x =>
                x.SenderUserId == otherUserId &&
                x.ReceiverUserId == userId &&
                !x.IsRead)
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsRead = true;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Mesajlar oxunmuş kimi işarələndi.",
            updatedCount = messages.Count
        });
    }
}