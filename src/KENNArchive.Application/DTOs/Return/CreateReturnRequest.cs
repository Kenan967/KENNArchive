namespace KENNArchive.Application.DTOs.Return;

// Müştərinin qaytarılma müraciəti üçün göndərdiyi məlumatlar.
public class CreateReturnRequest
{
    public string OrderNumber { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;
}