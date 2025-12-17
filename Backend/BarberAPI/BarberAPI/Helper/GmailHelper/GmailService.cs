using System.Net;
using System.Net.Mail;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace BarberAPI.Helper.GmailHelper
{
    public class GmailService : IMailService
    {
        private readonly GmailOptions _options;
        public GmailService(IOptions<GmailOptions> gmailOptions) 
        {
            _options = gmailOptions.Value;
        }

        public async Task SendEmailAsync(SendEmailRequest sendEmailRequest)
        {
            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress("barberapp910@gmail.com"),
                Subject = sendEmailRequest.Subject,
                Body = sendEmailRequest.Body,
            };

            mailMessage.To.Add(sendEmailRequest.Recipient);

            using var smtpClient = new SmtpClient();
            Console.WriteLine($"options\nHost:{_options.Host} Port:{_options.Port} " +
                $"Email:{_options.Email} Password:{_options.Password}");
            smtpClient.Host = _options.Host;
            smtpClient.Port = _options.Port;
            smtpClient.Credentials = new NetworkCredential(_options.Email, _options.Password);
            smtpClient.EnableSsl = true;

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
