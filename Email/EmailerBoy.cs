using Asgard;
using Microsoft.AspNetCore.Hosting;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Emailer
{
    public class EmailerBoy : IEmailer
    {
        private readonly IHeimdall GateKeeper;
        private readonly IHostingEnvironment environment;

        public EmailerBoy(IHeimdall configuration, IHostingEnvironment environment)
        {
            this.GateKeeper = configuration;
            this.environment = environment;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var apiKey = "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";// GateKeeper.GetSecretValue("SendGridApiKey");
            var client = new SendGridClient(apiKey);
            //var msg = new SendGridMessage()
            //{
            //    From = new EmailAddress("alok@planful.com"),
            //    Subject = subject,
            //    PlainTextContent = $"Hello, {toEmail}",
            //    HtmlContent = htmlMessage
            //};
            var msg = MailHelper.CreateSingleEmail(new EmailAddress("alok@unpaperworkz.in"), new EmailAddress(toEmail), subject, "Hello", htmlMessage);

            //msg.AddTo(new EmailAddress(toEmail, "Test User"));
            var response = await client.SendEmailAsync(msg);

        }

        public async Task SendAccountCreationMail(string Name , string toEmail, string url)
        {
            var fileInfo = environment.ContentRootFileProvider.GetFileInfo("wwwroot\\static\\Email\\NewAccount\\AccountCreation1.html");
            string htmlTemplate = string.Empty;
            using (var stream = fileInfo.CreateReadStream())
            {
                using (var streamReader = new StreamReader(stream))
                {
                    htmlTemplate = streamReader.ReadToEnd();
                }
                htmlTemplate = htmlTemplate.Replace("##NAME", Name);
                htmlTemplate = htmlTemplate.Replace("##URL", url);
                
            }
            await SendEmailAsync(toEmail, "Welcome to OnJob", htmlTemplate);
        }

        public async Task SendAccountConfirmationMail(string Name, string toEmail, string url)
        {
            string htmlTemplate = string.Empty;
            using (var stream = ReadEmailTemplate("wwwroot\\static\\Email\\NewAccount\\AccountConfrmSeparately.html"))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    htmlTemplate = streamReader.ReadToEnd();
                }
                htmlTemplate = htmlTemplate.Replace("##NAME", Name);
                htmlTemplate = htmlTemplate.Replace("##URL", url);

            }
            await SendEmailAsync(toEmail, "Confirm Your Account with On Job", htmlTemplate);
        }

        public async Task SendResetPasswordMail(string Name, string toEmail, string url)
        {
            string htmlTemplate = string.Empty;
            using (var stream = ReadEmailTemplate("wwwroot\\static\\Email\\NewAccount\\PasswordReset.html"))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    htmlTemplate = streamReader.ReadToEnd();
                }
                htmlTemplate = htmlTemplate.Replace("##NAME", Name);
                htmlTemplate = htmlTemplate.Replace("##URL", url);

            }
            await SendEmailAsync(toEmail, "Reset Password of On Job account", htmlTemplate);
        }


        public async Task SendNewOrderMail(Dictionary<string,string> templateReplace, string toEmail)
        {
            string htmlTemplate = string.Empty;
            using (var stream = ReadEmailTemplate("wwwroot\\static\\Email\\Case\\CustomerOrder.html"))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    htmlTemplate = streamReader.ReadToEnd();
                }
                foreach (var item in templateReplace)
                {
                    htmlTemplate = htmlTemplate.Replace(item.Key,item.Value);
                }


            }
            await SendEmailAsync(toEmail, $"Your OnJob Order #{templateReplace["##ORDERNO"]} Placed for {templateReplace["##SERVICE"]} has been placed", htmlTemplate);
        }

        public async Task SendCaseConfirmationEmail(Dictionary<string, string> templateReplace, string toEmail)
        {
            string htmlTemplate = string.Empty;
            using (var stream = ReadEmailTemplate("wwwroot\\static\\Email\\Consultant\\NewCaseConfirmation.html"))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    htmlTemplate = streamReader.ReadToEnd();
                }
                foreach (var item in templateReplace)
                {
                    htmlTemplate = htmlTemplate.Replace(item.Key, item.Value);
                }


            }
            await SendEmailAsync(toEmail, $"Your OnJob Case Acceptance for {templateReplace["##SERVICE"]} in city {templateReplace["##CITY"]}", htmlTemplate);
        }

        public Stream ReadEmailTemplate(string filePath)
        {
            var fileInfo = environment.ContentRootFileProvider.GetFileInfo(filePath);

            return fileInfo.CreateReadStream();
        }
    }
}
