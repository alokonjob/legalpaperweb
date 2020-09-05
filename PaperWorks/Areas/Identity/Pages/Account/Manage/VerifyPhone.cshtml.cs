using System;
using System.Threading.Tasks;
using Asgard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Phone;

using Twilio.Rest.Verify.V2.Service;
using Users;

namespace PaperWorks.Areas.Identity.Pages.Account
{
    [Authorize]
    public class VerifyPhoneModel : PageModel
    {
        private readonly IHeimdall configuration;
        private readonly UserManager<Clientele> _userManager;
        private readonly IPhoneService _phoneService;

        public VerifyPhoneModel(IHeimdall configuration, UserManager<Clientele> userManager, IPhoneService phoneService)
        {
            this.configuration = configuration;
            _userManager = userManager;
            _phoneService = phoneService;
        }

        public string PhoneNumber { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadPhoneNumber();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadPhoneNumber();

            try
            {
                var phoneDetails = await _phoneService.PhoneVerificationStatus(PhoneNumber);
                if (phoneDetails.VerificationStatusText == "pending")
                {
                    return RedirectToPage("ConfirmPhone");
                }

                ModelState.AddModelError("", $"There was an error sending the verification code: {phoneDetails.VerificationStatusText}");
            }
            catch (Exception error)
            {
                ModelState.AddModelError("",
                    "There was an error sending the verification code, please check the phone number is correct and try again");
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
    }
}