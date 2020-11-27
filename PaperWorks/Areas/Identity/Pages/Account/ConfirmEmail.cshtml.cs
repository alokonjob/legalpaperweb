using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Users;

namespace PaperWorks.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<Clientele> _userManager;

        public ConfirmEmailModel(UserManager<Clientele> userManager)
        {
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }
        public string ProceedLogin { get; set; }
        public IdentityResult ConfirmationResult { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(userId);
            if (user == null)
            {
                return RedirectToPage("Error");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            ConfirmationResult = await _userManager.ConfirmEmailAsync(user, code);
            StatusMessage = ConfirmationResult.Succeeded ? "Thank you for confirming your email" : "Error confirming your email.";
            ProceedLogin = ConfirmationResult.Succeeded ? "Please proceed to login." : "Please contact the administrator";
            return Page();
        }
    }
}
