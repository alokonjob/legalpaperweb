using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emailer
{
    public interface IEmailer
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
        Task SendAccountCreationMail(string Name , string toEmail, string url);
        Task SendAccountConfirmationMail(string Name, string toEmail, string url);
        Task SendResetPasswordMail(string Name, string toEmail, string url);
        Task SendNewOrderMail(Dictionary<string, string> templateReplace, string toEmail);
        Task SendCaseConfirmationEmail(Dictionary<string, string> templateReplace, string toEmail);


    }
}