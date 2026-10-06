namespace KENNArchive.Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeAsync(string email, string code);
}