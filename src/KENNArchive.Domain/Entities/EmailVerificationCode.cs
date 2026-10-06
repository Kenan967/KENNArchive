namespace KENNArchive.Domain.Entities;
public class EmailVerificationCode
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public ApplicationUser User { get; set; } = null!;
}