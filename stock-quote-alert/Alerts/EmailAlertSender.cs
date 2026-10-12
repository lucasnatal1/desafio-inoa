using System.Net;
using System.Net.Mail;
using Configuration;

namespace Alerts;

static class EmailAlertSender
{
    public static async Task SendEmailAsync(Config configurations, AlertKind kind, string stock, double price)
    {
        try
        {
            bool isSellType = kind == AlertKind.Sell;

            using MailMessage mail = new();
            mail.From = new MailAddress(configurations.Smtp.Username);
            mail.Subject = isSellType ? "Alerta de venda de ações" : "Alerta de compra de ações";
            mail.Body = isSellType ?
                $"A ação {stock} está valorizada (R${price:0.00}) e deve ser vendida."
                : $"A ação {stock} está desvalorizada (R${price:0.00}) e deve ser comprada.";
            mail.IsBodyHtml = false;

            foreach (string email in configurations.RecipientEmail)
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    mail.To.Add(email);
                }
            }

            using SmtpClient Smtp = new(configurations.Smtp.Host, configurations.Smtp.Port ?? 587);
            Smtp.Credentials = new NetworkCredential(configurations.Smtp.Username, configurations.Smtp.Password);
            Smtp.EnableSsl = configurations.Smtp.EnableSsl ?? true;
            Smtp.Timeout = 5000;

            await Smtp.SendMailAsync(mail);
            Console.WriteLine("Email sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email: {ex.Message}");
        }
    }
}