using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Address;
using Asgard;
using Audit;
using Emailer;
using Fundamentals.Events;
using Fundamentals.Managers;
using Fundamentals.Unit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using OrderAndPayments;
using Phone;
using Razorpay.Api;
using shortid;
using shortid.Configuration;
using Tax;
using User;
using Users;

namespace PaperWorks
{
    public class PreCheckout : PageModel
    {
        private readonly IEnabledServices enabledServicesManager;
        private readonly ITaxService taxService;
        private readonly IClienteleServices clientServices;
        private readonly UserManager<Clientele> userManager;
        private readonly SignInManager<Clientele> signInManager;
        private readonly IClienteleServices clienteleServices;
        private readonly CountryService countryService;
        private readonly IEmailer emailSender;
        private readonly IPhoneService phoneService;
        private readonly IOrderService orderService;
        private readonly IOrderAuditService orderAuditService;
        private readonly IOrderAuditService orderAudit;
        private readonly ILogger<PreCheckout> logger;

        public List<SelectListItem> AvailableCountries { get; }
        public string TaxAmount { get; set; }
        public string FinalAmount { get; set; }
        public EnabledServices CurrentOrderService = null;

        StringBuilder AuditString = new StringBuilder();
        /// <summary>
        /// This variable will be useful when we try to create a new user from this page and it fails.
        /// It will help in not disabling the UI elements, 
        /// which are otherwise disabled when input fields are non-empty on page load
        /// </summary>
        public bool UserCreationFailed { get; set; }
        public bool IsPhoneVerified { get; set; }
        public PreCheckout(IEnabledServices enabledServicesManager, ITaxService taxService,IClienteleServices clientServices, UserManager<Clientele> userManager,
            SignInManager<Clientele> signInManager,IClienteleServices clienteleServices,
            CountryService countryService, IEmailer emailSender, IPhoneService phoneService, IOrderService orderService, IOrderAuditService orderAuditService, ILogger<PreCheckout> logger)
        {
            this.enabledServicesManager = enabledServicesManager;
            this.taxService = taxService;
            this.clientServices = clientServices;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.clienteleServices = clienteleServices;
            this.countryService = countryService;
            this.emailSender = emailSender;
            this.phoneService = phoneService;
            this.orderService = orderService;
            this.orderAuditService = orderAuditService;
            this.logger = logger;
            AvailableCountries = countryService.GetCountries();
            AvailableCountries.Where(x => x.Value == "IN").FirstOrDefault().Selected = true;
            Input = new InputModel();
        }
        [BindProperty(SupportsGet = true)]
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
            public string PhoneCode { get; set; }
        }

        public string orderId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {

            //Detail Page passes this Enabled Service Id in Tempdata
            //We are saving it in session because we need this in Post Operation also.
            //Even when i try to peek tempdata it is not available in Post Call of same page.(do a research)
            var dbId = HttpContext.Session.GetString("DataBaseId");
            var callback = HttpContext.Session.GetInt32("bkc");
            if (!string.IsNullOrEmpty(dbId))
            {
                CurrentOrderService = enabledServicesManager.GetEnabledServiceById(dbId);
                if (isCallBack(callback))
                {
                    CurrentOrderService.CostToCustomer = 0.0;
                    CurrentOrderService.CostToConsultant = 0.0;
                }
                double currentApplicableTax = taxService.GetTaxAmount(CurrentOrderService.Location.State, CurrentOrderService.ServiceDetail.Name, CurrentOrderService.CostToCustomer);
                double finalAmount = currentApplicableTax + CurrentOrderService.CostToCustomer;
                TaxAmount = currentApplicableTax.ToString("#,##0.00");
                FinalAmount = finalAmount.ToString("#,##0.00");
            }
            else
            {
                return RedirectToPage("/index");
            }


            if (signInManager.IsSignedIn(User))
            {
                var signedInUser = userManager.GetUserAsync(User).Result;
                var phoneNumber = signedInUser.PhoneNumber != null ? signedInUser.PhoneNumber.Substring(signedInUser.PhoneNumber.Length - 10) : "";
                Input = new InputModel() { Name = signedInUser.FullName ?? "", Email = signedInUser.Email, PhoneNumber = phoneNumber };
                IsPhoneVerified = await userManager.IsPhoneNumberConfirmedAsync(signedInUser);
            }
            else
            {
                Input = new InputModel();
            }
            return Page();
        }

        public async Task<PartialViewResult> OnPostSendVerificationAsync()
        {
            try
            {
                var phoneNumber = await phoneService.ExtractPhoneNumber(Request.Form["verificationCountry"], Request.Form["verificationPhone"]);
                var phoneDetails = await phoneService.PhoneVerificationStatus(phoneNumber);
                if (phoneDetails.VerificationStatusText == "pending")
                {
                    return Partial("_VerificationCode", "");
                }
                return Partial("_Text", $"There was an error sending the verification code: {phoneDetails.VerificationStatusText}");
            }
            catch (Exception error)
            {
                logger.LogInformation($"Not able to send Verification code - {error.Message}");
            }

            return Partial("_Text", $"There was an error sending the verification code");
        }
        private bool isCallBack(int? bkc) => bkc % 2 == 0;

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                

                var dbId = HttpContext.Session.GetString("DataBaseId");
                int? cbk = HttpContext.Session.GetInt32("bkc");
                bool isCallBackRequested = isCallBack(cbk);
                CurrentOrderService = enabledServicesManager.GetEnabledServiceById(dbId);
                logger.LogInformation($"Precheckout.ServiceRequested.{CurrentOrderService.ServiceDetail.Name}.City.{CurrentOrderService.Location.City}.CallBack.{isCallBackRequested}");
                if (isCallBackRequested)
                {
                    CurrentOrderService.CostToCustomer = 0.0;
                    CurrentOrderService.CostToConsultant = 0.0;
                }
                double currentApplicableTax = taxService.GetTaxAmount(CurrentOrderService.Location.State, CurrentOrderService.ServiceDetail.Name, CurrentOrderService.CostToCustomer);
                double finalAmount = currentApplicableTax + CurrentOrderService.CostToCustomer;
                TaxAmount = currentApplicableTax.ToString("#,##0.00");
                FinalAmount = finalAmount.ToString("#,##0.00");

                //If User is not already signed in then create a login
                if (signInManager.IsSignedIn(User) == false)
                {
                    //create a new user with provided details
                    if (ModelState.IsValid)
                    {
                        string returnUrl = Url.Content("~/");
                        Clientele user = null;
                        //get a valid phone Number and save it while creating user
                        string phoneNumber = string.Empty; ;
                        try
                        {
                            phoneNumber = await phoneService.ExtractPhoneNumber(Input.PhoneNumberCountryCode, Input.PhoneNumber);
                            user = new Clientele { UserName = Input.Email, Email = Input.Email, IsActive = true,FullName = Input.Name, PhoneNumber = phoneNumber };
                        }
                        catch (Exception error)
                        {
                            logger.LogWarning(LogEvents.ErrorGetPhone, $"Error while Extracting Phone From Input CC {Input.PhoneNumberCountryCode},Phone {Input.PhoneNumber}, {error.Message}");
                        }

                        //If there is error while fetching phone details , user will not be created.
                        //so create a user without the phone
                        if (user == null)
                        {
                            user = new Clientele { UserName = Input.Email, Email = Input.Email, FullName = Input.Name, IsActive = true };
                        }

                        logger.LogInformation($"Precheckout.ServiceRequestedBy.{Input.Email}");
                        AuditString.AppendLine($"1.Precheckout.NotSingedIn.{Input.Email}.Service.{CurrentOrderService.ServiceDetail.Name}.City.{CurrentOrderService.Location.City}");


                        if (IsExistingUser(user) == true)
                        {
                            UserCreationFailed = true;
                            return Page();
                        }
                        if (user.PhoneNumberConfirmed == false)
                        {
                            string code = string.Empty;
                            try
                            {
                                code = Request.Form["code"];
                            }
                            catch (Exception error)
                            {
                                ModelState.Clear();
                                ModelState.AddModelError(string.Empty, "Unable To Read The Phone Verification Code");


                                UserCreationFailed = true; 
                                Input = new InputModel() { Name = user.FullName ?? "", Email = user.Email, PhoneNumber = phoneNumber };
                                return Page();
                            }
                            if (string.IsNullOrEmpty(code))
                            {
                                ModelState.Clear();
                                ModelState.AddModelError(string.Empty, "Phone Verification Code is Invalid");

                                UserCreationFailed = true; 
                                Input = new InputModel() { Name = user.FullName ?? "", Email = user.Email, PhoneNumber = phoneNumber };
                                return Page();
                            }

                            var phoneDetails = await phoneService.IsPhoneVerifiedAsync(phoneNumber, code);

                            if (phoneDetails.IsVerified == false)
                            {
                                ModelState.Clear();
                                ModelState.AddModelError(string.Empty, "Phone Verification Failed");

                                UserCreationFailed = true; 
                                Input = new InputModel() { Name = user.FullName ?? "", Email = user.Email, PhoneNumber = phoneNumber };

                                return Page();
                            }
                            else
                            {
                                user.PhoneNumberConfirmed = true;
                            }
                        }

                        var password = PasswordGenerator.GenerateRandomPassword();
                        var userCreationResult = await clientServices.CreateNewUserWithPassword(user, password);


                        if (userCreationResult.ResultValue == Fundamentals.ResultValue.Success)
                        {
                            logger.LogInformation(LogEvents.NewUserCreated, $"Precheckout.NewUserCreated.{Input.Email}.Success");
                            AuditString.AppendLine($"2.Precheckout.NewUserCreated.{Input.Email}.Success");
                            // _logger.LogInformation("User created a new account with password.");

                            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                            var callbackUrl = Url.Page(
                                "/Account/ConfirmEmail",
                                pageHandler: null,
                                values: new { area = "Identity", userId = Input.Email, code = code, returnUrl = returnUrl },
                                protocol: Request.Scheme);

                            var confirmEmailTask = emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                            var PassworkEmailTask = emailSender.SendEmailAsync(Input.Email, "Your password",
                                $"Please Use the below password to login.<br/> {password} <br/> This is your confidential information, please dont share with others or our staff.");


                            //signInManager.SignInAsync(user, isPersistent: false);
                            var loginResult = signInManager.PasswordSignInAsync(Input.Email, password, true, lockoutOnFailure: false);

                            await Task.WhenAll(new List<Task>() { confirmEmailTask, PassworkEmailTask, loginResult });

                            logger.LogInformation(LogEvents.PasswordSignInSuccess, $"Precheckout.SignIn.{Input.Email}.Success");
                            AuditString.AppendLine($"3.Precheckout.SignIn.{Input.Email}.Success");
                            if (loginResult.Result.Succeeded)
                            {
                                await CreateFreshOrder(isCallBackRequested,Input.Email);
                                return RedirectToPage("./Checkout");
                            }


                        }

                        foreach (var error in userCreationResult.ResultMessages)
                        {
                            ModelState.AddModelError(string.Empty, error);
                            UserCreationFailed = true;
                            logger.LogInformation(LogEvents.NewUserCreated, $"Precheckout.UserCreation.{Input.Email}.Fail.Message.{error}");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(dbId))
                        {
                            TaxAmount = currentApplicableTax.ToString();
                            FinalAmount = finalAmount.ToString();
                        }
                        return Page();
                    }
                }
                else if (signInManager.IsSignedIn(User))
                {
                    UserCreationFailed = false;//it is a hack , think of another graceful solution man
                    logger.LogInformation(LogEvents.NewUserCreated, $"Precheckout.UserCreation.{User.Identity.Name}.NotNeeded.SignedIn");
                    AuditString.AppendLine($"1.Precheckout.SignedIn.{User.Identity.Name}.Service.{CurrentOrderService.ServiceDetail.Name}.City.{CurrentOrderService.Location.City}");
                    var user = userManager.GetUserAsync(User).Result;


                        bool requiresUpdate = false;
                    if (string.IsNullOrEmpty(user.FullName))
                    {
                        user.FullName = Input.Name;
                        requiresUpdate = true;
                        AuditString.AppendLine($"1.1.Precheckout.Service.SignedIn.{User.Identity.Name}.NameTobeUpdated");
                    }
                    if (string.IsNullOrEmpty(user.PhoneNumber) || user.PhoneNumberConfirmed == false)
                    {
                        var phoneNumber = phoneService.ExtractPhoneNumber(Input.PhoneNumberCountryCode, Input.PhoneNumber?? user.PhoneNumber).Result;
                        user.PhoneNumber = phoneNumber;
                        requiresUpdate = true;
                        if (user.PhoneNumberConfirmed == false)
                        {
                            string code = string.Empty;
                            try
                            {
                                code = Request.Form["code"];
                            }
                            catch (Exception error)
                            {
                                ModelState.Clear();
                                ModelState.AddModelError(string.Empty, "Unable To Read The Phone Verification Code");


                                Input = new InputModel() { Name = user.FullName ?? "", Email = user.Email, PhoneNumber = phoneNumber };
                                return Page();
                            }
                            if (string.IsNullOrEmpty(code))
                            {
                                ModelState.Clear();
                                ModelState.AddModelError(string.Empty, "Phone Verification Code is Invalid");

                                Input = new InputModel() { Name = user.FullName ?? "", Email = user.Email, PhoneNumber = phoneNumber };
                                return Page();
                            }

                            var phoneDetails = await phoneService.IsPhoneVerifiedAsync(phoneNumber, code);

                            if (phoneDetails.IsVerified == false)
                            {
                                ModelState.Clear();
                                ModelState.AddModelError(string.Empty, "Phone Verification Failed");

                                Input = new InputModel() { Name = user.FullName ?? "", Email = user.Email, PhoneNumber = phoneNumber };

                                return Page();
                            }
                            else
                            {
                                user.PhoneNumberConfirmed = true;
                            }
                        }

                        AuditString.AppendLine($"1.2.Precheckout.Service.SignedIn.{Input.PhoneNumber}.PhoneTobeUpdated");
                    }

                    if (requiresUpdate)
                    {
                        var userUpdateResult = userManager.UpdateAsync(user).Result;
                        await signInManager.RefreshSignInAsync(user);
                    }
                    await CreateFreshOrder(isCallBackRequested);
                    return RedirectToPage("./Checkout");
                }

            }
            catch (Exception error)
            {
                logger.LogCritical(LogEvents.PreCheckoutError, $"Unknow Error in Precheckout {error.Message}");
            }
            finally 
            {
                OrderAudit audit = new OrderAudit();
                audit.Email = Input.Email??User.Identity.Name;
                audit.DateOfOrder = DateTime.UtcNow;
                audit.OrderReceipt = HttpContext.Session.GetString("OrderReceipt");
                audit.History = AuditString.ToString().Split('\n').ToList();
                await orderAuditService.AddAudit(audit);


            }
            //If user is already signed in but no phone. Ask to Enter Phone.
            //If User is already signed in and there phone/email verification and email verifi
            return Page();
        }

        private bool IsExistingUser(Clientele user)
        {
            var existingUserByPhone = userManager.Users.Where(item => item.PhoneNumber == user.PhoneNumber).FirstOrDefault();
            var existingUserByEmail = userManager.Users.Where(item => item.Email.ToLower() == Input.Email.ToLower()).FirstOrDefault();
            if (existingUserByPhone != null || existingUserByEmail != null)
            {
                if (existingUserByPhone != null)
                {
                    ModelState.AddModelError(string.Empty, $"Phone Number {user.PhoneNumber} Is Already Taken");
                }
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError(string.Empty, $"Email {user.Email} Is Already Taken");
                }


                return true;
            }
            return false;
        }

        public async Task CreateFreshOrder(bool  isCallBack ,string UserEmail = "")
        {
            ClienteleOrder order = new ClienteleOrder();
            order.CustomerRequirementDetail = CurrentOrderService;
            order.OrderPlacedOn = DateTime.UtcNow;
            var options = new GenerationOptions
            {
                UseNumbers = true,
                UseSpecialCharacters = false,
                Length = 10,
            };
            var userDetails = string.IsNullOrEmpty(UserEmail) ?  await clienteleServices.GetUserAsync(User) : await clienteleServices.GetByEmail(UserEmail);

            order.Receipt = ShortId.Generate(options).ToUpper();
            order.ClientId = userDetails.Id;
            order.OrderStatus = OrderStatus.Initiated;
            order.LinkOrderId = ObjectId.GenerateNewId();
            order.Type = isCallBack ? OrderType.FreeCallBack : OrderType.RegularOrder;
            var finalOrder = await orderService.SaveOrder(order);
            HttpContext.Session.SetString("OrderReceipt", finalOrder.Receipt);
            AuditString.AppendLine($"#.Precheckout.FreshOrder.OrderId.{finalOrder.ClientOrderId.ToString()}.Client.{userDetails.Id}");
        }

    }
}