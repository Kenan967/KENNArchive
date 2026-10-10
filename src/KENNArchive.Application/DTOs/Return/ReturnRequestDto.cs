namespace KENNArchive.Application.DTOs.Return;

// Qaytarılma müraciətinin cavab məlumatları.
public class ReturnRequestDto
{
    public string OrderNumber { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string AdminNote { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}