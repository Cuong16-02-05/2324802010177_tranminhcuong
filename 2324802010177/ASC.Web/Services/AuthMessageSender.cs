using ASC.Web.Configuration;
using Microsoft.Extensions.Options;

namespace ASC.Web.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

    public class AuthMessageSender : IEmailSender
    {
        private readonly ApplicationSettings _settings;
        public AuthMessageSender(IOptions<ApplicationSettings> options) { _settings = options.Value; }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var smtp = _settings.Smtp ?? new SmtpConfig { Host = _settings.SMTPServer, Port = _settings.SMTPPort, From = _settings.SMTPAccount, Password = _settings.SMTPPassword };
                using var client = new MailKit.Net.Smtp.SmtpClient();
                var msg = new MimeKit.MimeMessage();
                msg.From.Add(new MimeKit.MailboxAddress("ASC", smtp.From));
                msg.To.Add(new MimeKit.MailboxAddress("", email));
                msg.Subject = subject;
                msg.Body = new MimeKit.TextPart("html") { Text = message };
                await client.ConnectAsync(smtp.Host, smtp.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtp.From, smtp.Password);
                await client.SendAsync(msg);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex) { Console.WriteLine($"Email send failed: {ex.Message}"); }
        }
    }
}
