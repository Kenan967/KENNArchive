using System.Net;
using System.Net.Mail;
using KENNArchive.Application.Interfaces;
using Microsoft.Extensions.Configuration;
namespace KENNArchive.Infrastructure.Services;
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task SendVerificationCodeAsync(
        string email,
        string code)
    {
        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpPort = int.Parse(
            _configuration["Email:SmtpPort"]!);
        var smtpEmail = _configuration["Email:Email"];
        var smtpPassword = _configuration["Email:Password"];
        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(
                smtpEmail,
                smtpPassword)
        };
        using var message = new MailMessage(
            smtpEmail!,
            email);

        message.Subject = "KENN Archive - Email Verification";
        message.Body =
            $"Salam!\n\n" +
            $"Sizin KENN Archive təsdiq kodunuz: {code}\n\n" +
            $"Bu kod 5 dəqiqə ərzində etibarlıdır.";
        await client.SendMailAsync(message);
    }
}