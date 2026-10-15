namespace KENNArchive.Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeAsync(
    string email,
    string code);

Task SendOrderStatusUpdateAsync(
    string email,
    string orderNumber,
    string status);

}
