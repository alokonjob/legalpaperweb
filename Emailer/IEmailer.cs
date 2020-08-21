using System.Threading.Tasks;

namespace Emailer
{
    public interface IEmailer
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}