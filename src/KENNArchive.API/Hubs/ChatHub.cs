using System.Security.Claims;
using KENNArchive.Domain.Entities;
using KENNArchive.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KENNArchive.API.Hubs;

// Müştəri ilə mağaza sahibi arasında real vaxt mesajlaşması.
[Authorize]
public class ChatHub : Hub
{
    private readonly KennArchiveDbContext _context;
public ChatHub(KennArchiveDbContext context)
    {
        _context = context;
    }

    // İstifadəçi qoşulanda şəxsi mesaj qrupuna əlavə olunur.
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    // İstifadəçinin Admin rolunda olub-olmadığını yoxlayır.
    private async Task<bool> IsAdminAsync(int userId)
    {
        return await (
            from user in _context.Users
            join userRole in _context.UserRoles
                on user.Id equals userRole.UserId
            join role in _context.Roles
                on userRole.RoleId equals role.Id
            where user.Id == userId
                  && role.Name == "Admin"
            select user.Id
        ).AnyAsync();
    }

    // Göndərən və qəbul edən istifadəçiləri yoxlayır.
    private async Task<(int SenderId, int ReceiverId)> ValidateUsersAsync(
        string receiverUserId)
    {
        var senderUserId = Context.User?.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(senderUserId, out var senderId))
        {
            throw new HubException(
                "Göndərən istifadəçi müəyyən edilmədi.");
        }

        if (!int.TryParse(receiverUserId, out var receiverId))
        {
            throw new HubException(
                "Qəbul edən istifadəçinin ID-si yanlışdır.");
        }

        if (senderId == receiverId)
        {
            throw new HubException(
                "Özünüzə mesaj göndərə bilməzsiniz.");
        }

        var senderExists = await _context.Users
            .AnyAsync(x => x.Id == senderId);

        var receiverExists = await _context.Users
            .AnyAsync(x => x.Id == receiverId);

        if (!senderExists)
        {
            throw new HubException(
                "Göndərən istifadəçi tapılmadı.");
        }

        if (!receiverExists)
        {
            throw new HubException(
                "Qəbul edən istifadəçi tapılmadı.");
        }

        var senderIsAdmin = await IsAdminAsync(senderId);
        var receiverIsAdmin = await IsAdminAsync(receiverId);

        if (senderIsAdmin == receiverIsAdmin)
        {
            throw new HubException(
                "Mesajlaşma yalnız müştəri ilə mağaza sahibi arasında mümkündür.");
        }

        return (senderId, receiverId);
    }

    // Mətn mesajını database-də saxlayır və göndərir.
    public async Task SendMessage(
        string receiverUserId,
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new HubException(
                "Mesaj boş ola bilməz.");
        }

        message = message.Trim();

        if (message.Length > 2000)
        {
            throw new HubException(
                "Mesaj 2000 simvoldan uzun ola bilməz.");
        }

        var (senderId, receiverId) =
            await ValidateUsersAsync(receiverUserId);

        var chatMessage = new ChatMessage
        {
            SenderUserId = senderId,
            ReceiverUserId = receiverId,
            Message = message,
            ImageUrl = null,
            IsRead = false,
            SentAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        await Clients.Group($"user_{receiverId}")
            .SendAsync("ReceiveMessage", senderId, message);

        await Clients.Caller
            .SendAsync("ReceiveMessage", senderId, message);
    }

    // Cloudinary-dəki şəkil URL-sini mesaj kimi saxlayır və göndərir.
    public async Task SendImageMessage(
        string receiverUserId,
        string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new HubException(
                "Şəkil URL-si boş ola bilməz.");
        }

        if (!Uri.TryCreate(
                imageUrl,
                UriKind.Absolute,
                out var imageUri)
            || imageUri.Scheme != Uri.UriSchemeHttps
            || !imageUri.Host.Equals(
                "res.cloudinary.com",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new HubException(
                "Şəkil üçün etibarlı Cloudinary URL-si göndərin.");
        }

        var (senderId, receiverId) =
            await ValidateUsersAsync(receiverUserId);

        var chatMessage = new ChatMessage
        {
            SenderUserId = senderId,
            ReceiverUserId = receiverId,
            Message = string.Empty,
            ImageUrl = imageUrl,
            IsRead = false,
            SentAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        // Mövcud mətn hadisəsini qoruyuruq.
        // Şəkil URL-si üçün ayrıca hadisə göndəririk.
        await Clients.Group($"user_{receiverId}")
            .SendAsync(
                "ReceiveImageMessage",
                senderId,
                imageUrl);

        await Clients.Caller
            .SendAsync(
                "ReceiveImageMessage",
                senderId,
                imageUrl);
    }
}
