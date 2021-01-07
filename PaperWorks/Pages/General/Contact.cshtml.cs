using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emailer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using User;

namespace PaperWorks
{
    public class ContactModel : PageModel
    {
        private readonly IEmailer emailSender;

        public ContactModel(IEmailer emailSender)
        {
            this.emailSender = emailSender;
        }
        public void OnGet()
        {

        }

        [BindProperty(SupportsGet = true)]
        public ContactInput ContactForm { get; set; }

        public IActionResult OnPostContactUs()
        {

            emailSender.SendEmailAsync("aloksingh.itbhu@gmail.com", $"{ContactForm.Name} contacted with a Query", $"User { ContactForm.Email} and {ContactForm.PhoneNumber} has following Subject { ContactForm.Subject} with Message {ContactForm.Message}");
            return new OkResult();
        }
    }
}