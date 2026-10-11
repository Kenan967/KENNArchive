using System;
namespace KENNArchive.Domain.Entities;
// Müştəri və mağaza sahibi arasındakı mesajları saxlayır.
public class ChatMessage : BaseEntity
{
    // Mesajı göndərən istifadəçinin ID-si.
    public int SenderUserId { get; set; }
    // Mesajı qəbul edən istifadəçinin ID-si.
    public int ReceiverUserId { get; set; }
    // Mesajın mətni.
    public string Message { get; set; } = string.Empty;
    // Mesajın oxunub-oxunmadığını göstərir.
    public bool IsRead { get; set; } = false;

    // Mesajın göndərilmə vaxtı.
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}