using Application.Dtos.Email;
using Application.Interfaces;
using Domain.Settings;
using Microsoft.Extensions.Options;
using MimeKit;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings mailSettings;
        private readonly ILogger<EmailService> logger;

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
        {
            this.mailSettings = mailSettings.Value;
            this.logger = logger;
        }

        public async Task SendAsync(EmailRequestDto emailRequestDto)
        {
            try
            {
                emailRequestDto.ToRange?.Add(emailRequestDto.To ?? "");

                MimeMessage email = new()
                {
                    Sender = MailboxAddress.Parse(mailSettings.EmailFrom),
                    Subject = emailRequestDto.Subject
                };

                foreach (var toItem in emailRequestDto.ToRange ?? [])
                {
                    email.To.Add(MailboxAddress.Parse(toItem));
                }

                BodyBuilder builder = new()
                {
                    HtmlBody = emailRequestDto.HtmlBody
                };
                email.Body = builder.ToMessageBody();

                using MailKit.Net.Smtp.SmtpClient smtpClient = new();
                await smtpClient.ConnectAsync(mailSettings.SmtpHost, mailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(mailSettings.SmtpUser, mailSettings.SmtpPass);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occured {Exception}.", ex);
            }
        }
    }
}
