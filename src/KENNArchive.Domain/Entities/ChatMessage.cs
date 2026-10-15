using System;

namespace KENNArchive.Domain.Entities;

// Müştəri ilə mağaza sahibi arasındakı mesajları saxlayır.
public class ChatMessage : BaseEntity
{
    // Mesajı göndərən istifadəçinin ID-si.
    public int SenderUserId { get; set; }
    // Mesajı qəbul edən istifadəçinin ID-si.
    public int ReceiverUserId { get; set; }
    // Mətn mesajı. Şəkil mesajında boş qala bilər.
    public string Message { get; set; } = string.Empty;
    // Şəkil mesajıdırsa, Cloudinary URL-si burada saxlanılır.
    public string? ImageUrl { get; set; }
    // Mesajın oxunub-oxunmadığını göstərir.
    public bool IsRead { get; set; } = false;
    // Mesajın göndərilmə vaxtı.
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}