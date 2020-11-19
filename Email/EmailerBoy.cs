using Asgard;

using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace Emailer
{
    public class EmailerBoy : IEmailer
    {
        private readonly IHeimdall GateKeeper;

        public EmailerBoy(IHeimdall configuration)
        {
            this.GateKeeper = configuration;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var apiKey = GateKeeper.GetSecretValue("SendGridApiKey");
            var client = new SendGridClient(apiKey);
            //var msg = new SendGridMessage()
            //{
            //    From = new EmailAddress("alok@planful.com"),
            //    Subject = subject,
            //    PlainTextContent = $"Hello, {toEmail}",
            //    HtmlContent = htmlMessage
            //};
            var msg = MailHelper.CreateSingleEmail(new EmailAddress("alok@planful.com"), new EmailAddress(toEmail), subject, "Hello", htmlMessage);

            //msg.AddTo(new EmailAddress(toEmail, "Test User"));
            var response = await client.SendEmailAsync(msg);

        }
    }
}
