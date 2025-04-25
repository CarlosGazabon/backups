using System.Net;
using System.Net.Mail;
using inventio.Models;
using Microsoft.Extensions.Configuration;
using Inventio.Models;

namespace inventio.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(EmailSchema request)
        {
            var mail = "inventio.admin@ars-combinatoria.com";
            var pw = "nsjzktmqdaitiukp";
            var receiver = request.Receiver;
            var subject = request.Subject;
            var message = request.Message;

            var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mail, pw),
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };

            return smtpClient.SendMailAsync(
                new MailMessage(
                    from: mail,
                    to: receiver,
                    subject,
                    message
                )
            );
        }

        public Task SendEmailForgotPwdAsync(User user)
        {

            var url = $"{_configuration["URLs:FrontURL"]}/reset-password/{user.Token}";
            var mail = _configuration["EmailSettings:Mail"];
            var pw = _configuration["EmailSettings:Password"];
            var receiver = user.Email!;
            var subject = "Password Reset";

            var message = "<html><body>";
            message += "<h1>Password Reset</h1>";
            message += $"<p>Hello {user.Name} {user.LastName},</p>";
            message += "<p>You have requested to reset your password.</p>";
            message += "<p>Click on the link below to reset your password:</p>";
            message += $"<a href='{url}'>Reset Password</a>";
            message += "</body></html>";


            var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mail, pw),
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };

            return smtpClient.SendMailAsync(
                new MailMessage(
                    from: mail!,
                    to: receiver,
                    subject,
                    message
                )
                {
                    IsBodyHtml = true
                }
            );
        }

        public Task SendEmailInvitationAsync(User user, string pass)
        {
            var url = $"{_configuration["URLs:FrontURL"]}";
            var mail = _configuration["EmailSettings:Mail"];
            var pw = _configuration["EmailSettings:Password"];
            var receiver = user.Email!;
            var subject = "Invitation Inventio";

            var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mail, pw),
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };

            var message = "<html><body>";
            message += "<h3>Welcome to INVENTIO</h3>";
            message += $"<p>Hi, {user.Name} {user.LastName},</p>";
            message += $"<p>This is your username: {user.Email}</p>";
            message += $"<p>This is your password: {pass}</p>";
            message += $"<p>Please <a href='{url}'>click here</a> to login</p>";
            message += "</body></html>";

            return smtpClient.SendMailAsync(
                new MailMessage(
                    from: mail!,
                    to: receiver,
                    subject,
                    message
                )
                { IsBodyHtml = true }
            );
        }
    }
}