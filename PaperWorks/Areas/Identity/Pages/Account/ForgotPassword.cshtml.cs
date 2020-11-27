using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Emailer;
using Users;

namespace PaperWorks.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<Clientele> _userManager;
        private readonly IEmailer _emailSender;

        public ForgotPasswordModel(UserManager<Clientele> userManager, IEmailer emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    ModelState.AddModelError(string.Empty, user != null ? "Your Account is Not Confirmed. Please check your Inbox and Confirm Your Email First" : "Invalid Email");
                    return Page();
                }
                if (!(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    ModelState.AddModelError(string.Empty, user != null ? "Your Account is Not Confirmed. Please check your Inbox and Confirm Your Email First" : "Invalid Email");
                    var emailConfirmCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    emailConfirmCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmCode));
                    var confirmcallbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = Input.Email.ToLower(), code = emailConfirmCode, returnUrl = Url.Content("~/") },
                        protocol: Request.Scheme);

                    await _emailSender.SendAccountConfirmationMail(user.FullName, Input.Email, confirmcallbackUrl);
                    return Page();
                }


                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                await _emailSender.SendResetPasswordMail(user.FullName,Input.Email,HtmlEncoder.Default.Encode(callbackUrl));

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
