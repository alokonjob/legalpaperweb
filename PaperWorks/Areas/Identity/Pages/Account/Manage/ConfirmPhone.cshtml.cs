using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SMSer;
using Twilio.Rest.Verify.V2.Service;
using Users;

namespace PaperWorks.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class ConfirmPhoneModel : PageModel
    {
        private readonly UserManager<Clientele> _userManager;
        private readonly IConfiguration configuration;
        public bool IsPhoneNumberAlreadyConfirmed = false;
        public ConfirmPhoneModel(UserManager<Clientele> userManager, IConfiguration Configuration)
        {
            _userManager = userManager;
            configuration = Configuration;
        }

        public string PhoneNumber { get; set; }

        [BindProperty, Required, Display(Name = "Code")]
        public string VerificationCode { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            await LoadPhoneNumber();
            await LoadAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadPhoneNumber();
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var verification = await VerificationCheckResource.CreateAsync(
                    to: PhoneNumber,
                    code: VerificationCode,
                    pathServiceSid: configuration["TwilioVerificationServiceSID"]
                );
                if (verification.Status == "approved")
                {
                    var identityUser = await _userManager.GetUserAsync(User);
                    identityUser.PhoneNumberConfirmed = true;
                    var updateResult = await _userManager.UpdateAsync(identityUser);

                    if (updateResult.Succeeded)
                    {
                        return RedirectToPage("VerifyPhone");
                    }
                    else
                    {
                        ModelState.AddModelError("", "There was an error confirming the verification code, please try again");
                    }
                }
                else
                {
                    ModelState.AddModelError("", $"There was an error confirming the verification code: {verification.Status}");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("",
                    "There was an error confirming the code, please check the verification code is correct and try again");
            }

            return Page();
        }

        private async Task LoadPhoneNumber()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new Exception($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            PhoneNumber = user.PhoneNumber;
        }

        private async Task LoadAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            IsPhoneNumberAlreadyConfirmed = user.PhoneNumberConfirmed;
        }
    }
}