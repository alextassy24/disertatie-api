#nullable disable

using backend.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using static backend.Controllers.AccountController;

namespace backend.Services
{
    public interface IEmailService
    {
        Task<bool> SendConfirmationEmail(string email, string confirmationToken);
        Task<bool> SendPasswordResetEmail(string email, string resetToken);
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpConfiguration _smtpConfig;

        public EmailService(IOptions<SmtpConfiguration> smtpConfig)
        {
            _smtpConfig = smtpConfig.Value;
        }

        public async Task<bool> SendConfirmationEmail(string email, string confirmationToken)
        {
            try
            {
                var message = new MimeMessage();
                message.Sender = MailboxAddress.Parse(_smtpConfig.FromAddress);
                message.From.Add(new MailboxAddress(_smtpConfig.FromName, _smtpConfig.FromAddress));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = "Please Confirm Your Email";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody =
                        $"Please click the following link to confirm your email: <a href='{_smtpConfig.AppBaseUrl}/confirm-email?token={confirmationToken}'>Confirm Email</a>"
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(
                    _smtpConfig.SmtpServer,
                    _smtpConfig.Port,
                    SecureSocketOptions.StartTls
                );
                await client.AuthenticateAsync(_smtpConfig.Username, _smtpConfig.AppPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                return true; // Email sent successfully
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"Failed to send confirmation email: {ex.Message}");
            }
            return false; // Failed to send email
        }

        public async Task<bool> SendPasswordResetEmail(string email, string resetToken)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_smtpConfig.FromName, _smtpConfig.FromAddress));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = "Reset Your Password";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody =
                        $"Please click the following link to reset your password: <a href='{_smtpConfig.AppBaseUrl}/recover-password?token={resetToken}'>Reset Password</a>"
                };
                message.Body = bodyBuilder.ToMessageBody();
                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(
                    _smtpConfig.SmtpServer,
                    _smtpConfig.Port,
                    SecureSocketOptions.StartTls
                );
                await client.AuthenticateAsync(_smtpConfig.Username, _smtpConfig.AppPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine(ConsoleColors.Red($"Failed to password reset email: {ex.Message}"));
            }
            return false; // Failed to send email
        }
    }
}
