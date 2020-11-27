using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Address;
using Emailer;
using Fundamentals.Events;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Phone;
using Users;

namespace PaperWorks.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Clientele> _signInManager;
        private readonly UserManager<Clientele> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly CountryService countryService;
        private readonly IPhoneService phoneService;
        private readonly IEmailer _emailSender;

        public RegisterModel(
            UserManager<Clientele> userManager,
            SignInManager<Clientele> signInManager,
            ILogger<RegisterModel> logger, CountryService countryService, IPhoneService phoneService,
            IEmailer emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            this.countryService = countryService;
            this.phoneService = phoneService;
            _emailSender = emailSender;
            AvailableCountries = countryService.GetCountries();
            AvailableCountries.Where(x => x.Value == "IN").FirstOrDefault().Selected = true;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public List<SelectListItem> AvailableCountries { get; }
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
            [Required]
            public string Name { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Phone Number")]
            [RegularExpression(@"^([1-9][0-9]{9})$", ErrorMessage = "Invalid Phone Number.")]
            [Display(Name = "Mobile number")]
            public string PhoneNumber { get; set; }
            [Display(Name = "Phone number country")]
            public string PhoneNumberCountryCode { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                Clientele user = null;
                //get a valid phone Number and save it while creating user
                try
                {
                    var phoneNumber = await phoneService.ExtractPhoneNumber(Input.PhoneNumberCountryCode, Input.PhoneNumber);
                    user = new Clientele { UserName = Input.Email, Email = Input.Email, IsActive = true, FullName = Input.Name, PhoneNumber = phoneNumber };
                }
                catch (Exception error)
                {
                    _logger.LogWarning(LogEvents.ErrorGetPhone, $"Error while Extracting Phone From Input CC {Input.PhoneNumberCountryCode},Phone {Input.PhoneNumber}, {error.Message}");
                }

                //If there is error while fetching phone details , user will not be created.
                //so create a user without the phone
                if (user == null)
                {
                    user = new Clientele { UserName = Input.Email, Email = Input.Email,FullName = Input.Name, IsActive = true };
                }

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = Input.Email, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendAccountCreationMail(Input.Name,Input.Email,HtmlEncoder.Default.Encode(callbackUrl));

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
