using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Address;
using Emailer;
using Fundamentals.Managers;
using Fundamentals.Unit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Phone;
using Razorpay.Api;
using Tax;
using User;
using Users;

namespace PaperWorks
{
    public class PreCheckout : PageModel
    {
        private readonly IEnabledServices enabledServicesManager;
        private readonly ITaxService taxService;
        private readonly UserManager<Clientele> userManager;
        private readonly SignInManager<Clientele> signInManager;
        private readonly CountryService countryService;
        private readonly IEmailer emailSender;
        private readonly IPhoneService phoneService;

        public List<SelectListItem> AvailableCountries { get; }
        public string TaxAmount { get; set; }
        public string FinalAmount { get; set; }
        public EnabledServices CurrentOrderService = null;

        /// <summary>
        /// This variable will be useful when we try to create a new user from this page and it fails.
        /// It will help in not disabling the UI elements, 
        /// which are otherwise disabled when input fields are non-empty on page load
        /// </summary>
        public bool UserCreationFailed { get; set; }
        public PreCheckout(IEnabledServices enabledServicesManager, ITaxService taxService, UserManager<Clientele> userManager,
            SignInManager<Clientele> signInManager,
            CountryService countryService, IEmailer emailSender,IPhoneService phoneService)
        {
            this.enabledServicesManager = enabledServicesManager;
            this.taxService = taxService;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.countryService = countryService;
            this.emailSender = emailSender;
            this.phoneService = phoneService;
            AvailableCountries = countryService.GetCountries();
            AvailableCountries.Where(x => x.Value == "IN").FirstOrDefault().Selected = true;
            Input = new InputModel();
        }
        [BindProperty(SupportsGet =true)]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Phone Number")]
            [RegularExpression(@"^([1-9][0-9]{9})$", ErrorMessage = "Invalid Phone Number.")]
            [Display(Name = "Mobile number")]
            public string PhoneNumber { get; set; }
            [Display(Name = "Phone number country")]
            public string PhoneNumberCountryCode { get; set; }
        }

        public string orderId { get; set; }

        public void OnGetAsync()
        {
            
            //Detail Page passes this Enabled Service Id in Tempdata
            //We are saving it in session because we need this in Post Operation also.
            //Even when i try to peek tempdata it is not available in Post Call of same page.(do a research)
            var dbId= HttpContext.Session.GetString("DataBaseId");

            if (!string.IsNullOrEmpty(dbId))
            {
                CurrentOrderService = enabledServicesManager.GetEnabledServiceById(dbId);
                double currentApplicableTax = taxService.GetTaxAmount(CurrentOrderService.Location.State, CurrentOrderService.ServiceDetail.Name, CurrentOrderService.CostToCustomer);
                double finalAmount = currentApplicableTax + CurrentOrderService.CostToCustomer;
                TaxAmount = currentApplicableTax.ToString();
                FinalAmount = finalAmount.ToString();
            }

            if (signInManager.IsSignedIn(User))
            {
                var signedInUser = userManager.GetUserAsync(User).Result;
                var phoneNumber = signedInUser.PhoneNumber != null ? signedInUser.PhoneNumber.Substring(signedInUser.PhoneNumber.Length - 10) : "";
                Input = new InputModel() { Name = signedInUser.FullName ??"", Email = signedInUser.Email, PhoneNumber = phoneNumber };
            }
            else
            {
                Input = new InputModel();
            }
        }

        public IActionResult OnPostAsync()
        {
            var dbId = HttpContext.Session.GetString("DataBaseId");
            //If User is not already signed in then create a login
            if (signInManager.IsSignedIn(User) == false)
            {
                //create a new user with provided details
                if (ModelState.IsValid)
                {
                    string returnUrl =  Url.Content("~/");
                    Clientele user = null;
                    //get a valid phone Number and save it while creating user
                    try
                    {
                        var phoneNumber = phoneService.ExtractPhoneNumber(Input.PhoneNumberCountryCode, Input.PhoneNumber).Result;
                        user = new Clientele { UserName = Input.Email, Email = Input.Email, IsActive = true, PhoneNumber = phoneNumber };
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error.Message);
                    }
                    //If there is error while fetching phone details , user will not be created.
                    //so create a user without the phone
                    if (user == null)
                    {
                        user = new Clientele { UserName = Input.Email, Email = Input.Email, IsActive = true};
                    }
                    var password = PasswordGenerator.GenerateRandomPassword();
                    var existingUserByPhone = userManager.Users.Where(item => item.PhoneNumber == user.PhoneNumber).FirstOrDefault();
                    if (existingUserByPhone != null)
                    {
                        ModelState.AddModelError(string.Empty, $"Phone Number {user.PhoneNumber} Is Already Taken");
                        CurrentOrderService = enabledServicesManager.GetEnabledServiceById(dbId);
                        UserCreationFailed = true;
                        return Page();
                    }
                    var result = userManager.CreateAsync(user, password).Result;
                    if (result.Succeeded)
                    {
                       // _logger.LogInformation("User created a new account with password.");

                        var code = userManager.GenerateEmailConfirmationTokenAsync(user).Result;
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                        emailSender.SendEmailAsync(Input.Email, "Your password",
                            $"Please Use the below password to login.<br/> {password} <br/> This is your confidential information, please dont share with others or our staff.");

                        
                            //signInManager.SignInAsync(user, isPersistent: false);
                            var loginResult = signInManager.PasswordSignInAsync(Input.Email, password, true, lockoutOnFailure: false).Result;
                            if (loginResult.Succeeded)
                            {
                               // TempData["DataBaseId"] = HttpContext.Session.GetString("DataBaseId");
                                //var dbId = ViewData["MyNumber"];// TempData.Peek("DataBaseId").ToString();
                                return RedirectToPage("./Checkout");
                            }
                            
                        
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        CurrentOrderService = enabledServicesManager.GetEnabledServiceById(dbId);
                        UserCreationFailed = true;
                    }
                }
                else {
                    if (!string.IsNullOrEmpty(dbId))
                    {
                        CurrentOrderService = enabledServicesManager.GetEnabledServiceById(dbId);
                        double currentApplicableTax = taxService.GetTaxAmount(CurrentOrderService.Location.State, CurrentOrderService.ServiceDetail.Name, CurrentOrderService.CostToCustomer);
                        double finalAmount = currentApplicableTax + CurrentOrderService.CostToCustomer;
                        TaxAmount = currentApplicableTax.ToString();
                        FinalAmount = finalAmount.ToString();
                    }
                    return Page();
                }
            }
            else if (signInManager.IsSignedIn(User))
            {
                UserCreationFailed = false;//it is a hack , think of another graceful solution man
                var user = userManager.GetUserAsync(User).Result;
                bool requiresUpdate = false;
                if (string.IsNullOrEmpty(user.FullName))
                {
                    user.FullName = Input.Name;
                    requiresUpdate = true;
                }
                if (string.IsNullOrEmpty(user.PhoneNumber))
                {
                    var phoneNumber = phoneService.ExtractPhoneNumber(Input.PhoneNumberCountryCode, Input.PhoneNumber).Result;
                    user.PhoneNumber = phoneNumber;
                    requiresUpdate = true;
                }

                if (requiresUpdate)
                {
                   var userUpdateResult= userManager.UpdateAsync(user).Result;
                    signInManager.RefreshSignInAsync(user);
                }
                              
                return RedirectToPage("./Checkout");
            }
            //If user is already signed in but no phone. Ask to Enter Phone.
            //If User is already signed in and there phone/email verification and email verifi
            return Page();
        }
        
    }
}