using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Emailer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using User;
using Users;

namespace PaperWorks
{
    [Authorize(Policy = "AddEditPeople")]
    public class StaffManagementModel : PageModel
    {
        private readonly SignInManager<Clientele> _signInManager;
        private readonly IClienteleServices clientServices;
        private readonly UserManager<Clientele> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailer _emailSender;

        public StaffManagementModel(
            IClienteleServices clientServices,
            UserManager<Clientele> userManager,
            SignInManager<Clientele> signInManager,
            ILogger<RegisterModel> logger,
            IEmailer emailSender)
        {
            this.clientServices = clientServices;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public Clientele StaffUser { get; set; }
        public string Email { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Role")]
            public string Role { get; set; }

            [DataType(DataType.PostalCode)]
            [Display(Name = "Add claims(comma separated)")]
            public string Claims { get; set; }
        }

        public async Task OnGetAsync(string userEmail = null)
        {
            StaffUser = userEmail != null ? await _userManager.FindByEmailAsync(userEmail) : null;
        }
        public async Task<IActionResult> OnPostDeleteUserAsync()
        {
            string Email = Request.Form["Email"];
            Clientele userToDelete = await _userManager.FindByEmailAsync(Input.Email);
            await _userManager.DeleteAsync(userToDelete);
            Input.Email = string.Empty;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                StaffUser = await _userManager.FindByEmailAsync(Input.Email);
                
                //If a user exists then we will try to update the user with those roles and claims
                //which are not already added

                if (StaffUser != null)
                {
                    if (Input.Role != null)
                    {
                        
                        if (_userManager.IsInRoleAsync(StaffUser,Input.Role).Result == false)
                        {
                            await _userManager.AddToRoleAsync(StaffUser, Input.Role);
                            //StaffUser.Roles.Add(Input.Role);
                        }
                    }

                    if (Input.Claims != null)
                    {
                        string[] claims = Input.Claims.Split(",");
                        List<Claim> allClaimsToAdd = new List<Claim>();
                        foreach (var claim in claims)
                        {
                            //IdentityUserClaim<string> identityClaim = new IdentityUserClaim<string>()
                            //{
                            //    ClaimType = "access",
                            //    ClaimValue = claim
                            //};

                            var newClaimToAdd = new Claim("access", claim);
                            

                            //if (StaffUser.Claims.Any(x=>x.ClaimValue == claim) == false)
                            //{
                                
                            //    StaffUser.Claims.Add(identityClaim);
                            //}
                            var allExistingClaims = _userManager.GetClaimsAsync(StaffUser).Result;
                            var isExistingClaim = allExistingClaims.Where(x => x.Type == newClaimToAdd.Type && x.Value == newClaimToAdd.Value).FirstOrDefault();
                            if (isExistingClaim == null) await _userManager.AddClaimAsync(StaffUser, newClaimToAdd);
                        }
                        

                    }
                    var updatedResult = await _userManager.UpdateAsync(StaffUser);
                    return Page();
                }

                var user =new Clientele { UserName = Input.Email, Email = Input.Email, IsActive = true };

                //if (Input.Role != null) user.Roles.Add(Input.Role);
                if (Input.Claims != null)
                {
                    string[] claims = Input.Claims.Split(",");

                    foreach (var claim in claims)
                    {
                        user.Claims.Add(new IdentityUserClaim<string>()
                        {
                            ClaimType = "access",
                            ClaimValue = claim
                        });
                    }
                }

                var password = PasswordGenerator.GenerateRandomPassword();
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Input.Role);

                    #region sendAccontConfirmationEmail
                    var code = clientServices.GetEmailConfirmationCode(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await clientServices.SendStaffAccountConfirmEmailOnLoginCreation(Input.Email, callbackUrl);
                    #endregion
                    #region SendPasswordEmail
                    await clientServices.SendNewPassword(Input.Email, password);
                    #endregion
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                StaffUser = await _userManager.FindByEmailAsync(Input.Email);
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteRoleAsync()
        {
            string Email = Request.Form["Email"];
            StaffUser = await _userManager.FindByEmailAsync(Email);

            //serManager.RemoveFromRoleAsync is not working
            StaffUser.Roles.Remove(Request.Form["RoleValue"].ToString());
            await _userManager.RemoveFromRoleAsync(StaffUser, Request.Form["RoleValue"]);
            StaffUser = await _userManager.FindByEmailAsync(Email);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteClaimAsync()
        {
            string Email = Request.Form["Email"];
            StaffUser = await _userManager.FindByEmailAsync(Email);
            await _userManager.RemoveClaimAsync(StaffUser, new System.Security.Claims.Claim(Request.Form["ClaimType"].ToString(), Request.Form["ClaimValue"].ToString()));
            StaffUser = await _userManager.FindByEmailAsync(Email);
            return Page();
        }
    }
}
