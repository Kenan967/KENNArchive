namespace KENNArchive.Domain.Entities;
// Müştərinin sifariş üçün qaytarılma müraciətini saxlayır.
public class ReturnRequest : BaseEntity
{
    // Qaytarılacaq sifariş
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    // Müraciəti edən istifadəçi
    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }

    // Müştərinin qaytarılma səbəbi
    public string Reason { get; set; } = string.Empty;
    // Müraciətin statusu
    public string Status { get; set; } = "Pending";
    // Adminin qeydi
    public string AdminNote { get; set; } = string.Empty;
}