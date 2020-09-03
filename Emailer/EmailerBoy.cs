using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace Emailer
{
    public class EmailerBoy : IEmailer
    {
        private readonly IConfiguration Configuration;

        public EmailerBoy(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var apiKey = Configuration["SendGridApiKey"];
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("alok@planful.com"),
                Subject = subject,
                PlainTextContent = $"Hello, {toEmail}",
                HtmlContent = htmlMessage
            };
            msg.AddTo(new EmailAddress(toEmail, "Test User"));
            var response = await client.SendEmailAsync(msg);

        }
    }
}
