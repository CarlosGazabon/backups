using inventio.Models;
using Inventio.Models;

namespace inventio.Services.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailSchema request);
        Task SendEmailForgotPwdAsync(User user);

        Task SendEmailInvitationAsync(User user, string pass);
    }
}