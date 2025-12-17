using MailKit.Security;
using MimeKit;
using recycle.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Infrastructure.ExternalServices
{
    public class EmailService:IEmailService
    {
        public async Task SendEmail(string recipient, string subject, string body)
        {
            var email = new MimeMessage()
            {
                Sender = MailboxAddress.Parse("mohammedmu7.20@gmail.com"),
                Subject = subject,
            };

            email.To.Add(MailboxAddress.Parse(recipient));
            email.From.Add(MailboxAddress.Parse("mohammedmu7.20@gmail.com"));

            var emailBody = new BodyBuilder();
            emailBody.TextBody = body;
            email.Body = emailBody.ToMessageBody();

            using var Smtp = new SmtpClient();
            await Smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            // Note: Ethereal is a fake SMTP service for testing purposes.
            await Smtp.AuthenticateAsync("mohammedmu7.20@gmail.com", "orui uorp kqgk phck");
            await Smtp.SendAsync(email);
            await Smtp.DisconnectAsync(true);
        }
    }
}
