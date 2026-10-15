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

    // İstifadəçiyə email təsdiq kodu göndərir.
    public async Task SendVerificationCodeAsync(
        string email,
        string code)
    {
        using var message = CreateMessage(email);

        message.Subject = "KENNArchive - Email Verification";
        message.Body =
            "Salam!\n\n" +
            $"Sizin KENNArchive təsdiq kodunuz: {code}\n\n" +
            "Bu kod yalnız 2 dəqiqə ərzində etibarlıdır!";

        await SendEmailAsync(message);
    }

    // Sifariş statusu dəyişəndə premium dizaynlı email göndərir.
    public async Task SendOrderStatusUpdateAsync(
        string email,
        string orderNumber,
        string status)
    {
        using var message = CreateMessage(email);

        message.Subject = "KENNArchive | Sifariş statusu yeniləndi";
        message.IsBodyHtml = true;

        var safeOrderNumber = WebUtility.HtmlEncode(orderNumber);
        var safeStatus = WebUtility.HtmlEncode(GetStatusText(status));
        var statusColor = GetStatusColor(status);

        message.Body = $@"
<!DOCTYPE html>
<html lang=""az"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>KENNArchive - Sifariş yeniləməsi</title>
</head>

<body style=""
    margin: 0;
    padding: 0;
    background-color: #f1f1ef;
    font-family: Arial, Helvetica, sans-serif;
    color: #171717;
"">

    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0""
           style=""background-color: #f1f1ef; padding: 35px 12px;"">
        <tr>
            <td align=""center"">

                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0""
                       style=""max-width: 560px; background-color: #ffffff;"">

                    <!-- Brend başlığı -->
                    <tr>
                        <td style=""
                            background-color: #111111;
                            padding: 34px 25px;
                            text-align: center;
                        "">
                            <div style=""
                                color: #ffffff;
                                font-size: 27px;
                                font-weight: bold;
                                letter-spacing: 5px;
                            "">
                                KENNARCHIVE
                            </div>

                            <div style=""
                                color: #b7b7b7;
                                font-size: 10px;
                                letter-spacing: 3px;
                                margin-top: 10px;
                            "">
                                CONTEMPORARY STREETWEAR
                            </div>
                        </td>
                    </tr>

                    <!-- Əsas məzmun -->
                    <tr>
                        <td style=""padding: 35px 28px 15px;"">

                            <div style=""
                                font-size: 11px;
                                color: #777777;
                                letter-spacing: 2px;
                                text-transform: uppercase;
                            "">
                                SİFARİŞ BİLDİRİŞİ
                            </div>

                            <h1 style=""
                                font-size: 25px;
                                line-height: 1.3;
                                margin: 15px 0;
                                color: #111111;
                            "">
                                Sifarişiniz yeniləndi.
                            </h1>

                            <p style=""
                                font-size: 14px;
                                line-height: 1.8;
                                color: #555555;
                                margin: 0 0 25px;
                            "">
                                Salam! Sifarişinizin cari statusunda dəyişiklik edilib.
                                Aşağıda sifariş məlumatlarınızı görə bilərsiniz.
                            </p>

                            <!-- Sifariş nömrəsi -->
                            <div style=""
                                border: 1px solid #e5e5e5;
                                padding: 20px;
                                margin-bottom: 18px;
                                background-color: #fafafa;
                            "">
                                <div style=""
                                    font-size: 10px;
                                    color: #777777;
                                    letter-spacing: 2px;
                                    margin-bottom: 10px;
                                "">
                                    SİFARİŞ NÖMRƏSİ
                                </div>

                                <div style=""
                                    font-size: 15px;
                                    font-weight: bold;
                                    color: #111111;
                                    overflow-wrap: anywhere;
                                    word-break: break-word;
                                "">
                                    #{safeOrderNumber}
                                </div>
                            </div>

                            <!-- Status -->
                            <div style=""
                                border: 1px solid #e5e5e5;
                                padding: 20px;
                                margin-bottom: 25px;
                            "">
                                <div style=""
                                    font-size: 10px;
                                    color: #777777;
                                    letter-spacing: 2px;
                                    margin-bottom: 12px;
                                "">
                                    YENİ STATUS
                                </div>

                                <span style=""
                                    display: inline-block;
                                    background-color: {statusColor};
                                    color: #ffffff;
                                    padding: 10px 15px;
                                    font-size: 13px;
                                    font-weight: bold;
                                    letter-spacing: 0.5px;
                                "">
                                    {safeStatus}
                                </span>
                            </div>

                            <p style=""
                                font-size: 13px;
                                line-height: 1.8;
                                color: #666666;
                            "">
                                Sifarişinizlə bağlı sualınız varsa, KENNArchive mağazası
                                ilə əlaqə saxlaya bilərsiniz.
                            </p>

                        </td>
                    </tr>

                    <!-- Alt hissə -->
                    <tr>
                        <td style=""
                            background-color: #f8f8f6;
                            padding: 25px 20px;
                            text-align: center;
                            border-top: 1px solid #eeeeec;
                        "">
                            <div style=""
                                font-size: 12px;
                                font-weight: bold;
                                letter-spacing: 3px;
                                color: #222222;
                            "">
                                KENNARCHIVE
                            </div>

                            <p style=""
                                font-size: 11px;
                                color: #888888;
                                line-height: 1.7;
                                margin: 12px 0 0;
                            "">
                                Seçiminiz üçün təşəkkür edirik.<br>
                                Bu, sifarişinizin statusu ilə bağlı avtomatik bildirişdir.
                            </p>
                        </td>
                    </tr>

                </table>

            </td>
        </tr>
    </table>

</body>
</html>";

        await SendEmailAsync(message);
    }

    // Email göndərmək üçün göndərənin ünvanını konfiqurasiyadan götürür.
    private MailMessage CreateMessage(string recipientEmail)
    {
        var smtpEmail = _configuration["Email:Email"];

        if (string.IsNullOrWhiteSpace(smtpEmail))
        {
            throw new InvalidOperationException(
                "Email:Email konfiqurasiyası tapılmadı.");
        }

        return new MailMessage(smtpEmail, recipientEmail);
    }

    // SMTP vasitəsilə emaili göndərir.
    private async Task SendEmailAsync(MailMessage message)
    {
        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpPortValue = _configuration["Email:SmtpPort"];
        var smtpEmail = _configuration["Email:Email"];
        var smtpPassword = _configuration["Email:Password"];

        if (string.IsNullOrWhiteSpace(smtpHost) ||
            !int.TryParse(smtpPortValue, out var smtpPort) ||
            string.IsNullOrWhiteSpace(smtpEmail) ||
            string.IsNullOrWhiteSpace(smtpPassword))
        {
            throw new InvalidOperationException(
                "Email SMTP konfiqurasiyası tam deyil.");
        }

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(
                smtpEmail,
                smtpPassword)
        };

        await client.SendMailAsync(message);
    }

    // Statusları emaildə Azərbaycan dilində göstərir.
    private static string GetStatusText(string status)
    {
        return status switch
        {
            "Pending" => "Gözləyir",
            "Confirmed" => "Təsdiqlənib",
            "Preparing" => "Hazırlanır",
            "Shipped" => "Göndərilib",
            "Delivered" => "Çatdırılıb",
            "Cancelled" => "Ləğv edilib",
            _ => status
        };
    }

    // Hər status üçün uyğun rəng seçir.
    private static string GetStatusColor(string status)
    {
        return status switch
        {
            "Pending" => "#8a6d3b",
            "Confirmed" => "#365f91",
            "Preparing" => "#69529a",
            "Shipped" => "#286b75",
            "Delivered" => "#34734b",
            "Cancelled" => "#a33d3d",
            _ => "#333333"
        };
    }
}
