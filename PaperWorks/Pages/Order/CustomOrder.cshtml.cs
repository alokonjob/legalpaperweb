using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Address;
using Audit;
using Emailer;
using Fundamentals;
using Fundamentals.Events;
using Fundamentals.Managers;
using Fundamentals.Unit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using OrderAndPayments;
using Phone;
using shortid;
using shortid.Configuration;
using User;
using Users;

namespace PaperWorks
{
    public class CustomOrderModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public InputModel Input { get; set; }
        public bool UserCreationFailed { get; set; }
        [BindProperty]
        public EnabledServices InputEnableService { get; set; }
        public string Receipt { get; set; }

        public List<SelectListItem> ServiceSelection;
        public List<SelectListItem> GeoSelection;
        private readonly IServiceManagement serviceManagement;
        private readonly IGeographyManagement geoManager;
        private readonly IEnabledServices enableService;
        private readonly IPhoneService phoneService;
        private readonly UserManager<Clientele> userManager;
        private readonly IClienteleServices clientServices;
        private readonly IEmailer emailSender;
        private readonly IOrderService orderService;
        private readonly IOrderAuditService orderAuditService;
        private readonly ILogger<CustomOrderModel> logger;

        public List<SelectListItem> AvailableCountries { get; }
        StringBuilder AuditString = new StringBuilder();
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
            [Required]
            [Display(Name = "Customer Cost")]
            public string CostToCustomer { get; set; }
            [Required]
            [Display(Name = "Consultant Cost")]
            public string CostToConsultant { get; set; }

            [Display(Name = "Link Order")]
            public string LinkOrderId { get; set; }

        }

        public CustomOrderModel(IServiceManagement serviceManagement, IGeographyManagement geoManager, IEnabledServices enableService, CountryService countryService,
            IPhoneService phoneService, UserManager<Clientele> userManager, IClienteleServices clientServices, IEmailer emailSender, IOrderService orderService, IOrderAuditService orderAuditService,ILogger<CustomOrderModel> logger)
        {
            this.serviceManagement = serviceManagement;
            this.geoManager = geoManager;
            this.enableService = enableService;
            this.phoneService = phoneService;
            this.userManager = userManager;
            this.clientServices = clientServices;
            this.emailSender = emailSender;
            this.orderService = orderService;
            this.orderAuditService = orderAuditService;
            this.logger = logger;
            var AllServices = serviceManagement.FetchAllServices();
            var AllGeographies = geoManager.FetchAllGeographies();
            ServiceSelection = AllServices.Select(x => new SelectListItem(x.Name, x.Name)).ToList();
            GeoSelection = AllGeographies.Select(x => new SelectListItem(x.City, x.City)).ToList();
            AvailableCountries = countryService.GetCountries();
            AvailableCountries.Where(x => x.Value == "IN").FirstOrDefault().Selected = true;
        }
        public void OnGet()
        {

        }
        public async Task<IActionResult> OnPost()
        {
            try
            {
                Clientele user = null;
                string returnUrl = Url.Content("~/");
                try
                {
                    var phoneNumber = await phoneService.ExtractPhoneNumber(Input.PhoneNumberCountryCode, Input.PhoneNumber);
                    user = new Clientele { UserName = Input.Email, Email = Input.Email, IsActive = true, FullName = Input.Name, PhoneNumber = phoneNumber };
                }
                catch (Exception error)
                {
                    logger.LogWarning(LogEvents.ErrorGetPhone, $"Error while Extracting Phone From Input CC {Input.PhoneNumberCountryCode},Phone {Input.PhoneNumber}, {error.Message}");
                }
                logger.LogInformation($"CustomOrderService.RequestedBy.{User.Identity.Name}.ForUser.{Input.Email}.Service.{InputEnableService.ServiceDetail.Name}.city.{InputEnableService.Location.City}");
                AuditString.AppendLine($"1.CustomOrderService.RequestedBy.{ User.Identity.Name}.ForUser.{ Input.Email}.Service.{ InputEnableService.ServiceDetail.Name}.city.{ InputEnableService.Location.City}");

                //If there is error while fetching phone details , user will not be created.
                //so create a user without the phone
                if (user == null)
                {
                    user = new Clientele { UserName = Input.Email, Email = Input.Email, FullName = Input.Name, IsActive = true };
                }

                //AuditString.AppendLine($"1.CustomOrderNotSingedIn.{Input.Email}.Service.{CurrentOrderService.ServiceDetail.Name}.City.{CurrentOrderService.Location.City}");


                if (IsExistingUser(user) == true)
                {
                    if (VerifyExistingUserMismatch(user))
                    {
                        return Page();
                    }
                }
                else
                {
                    logger.LogInformation($"CustomOrderService.User.{Input.Email}.WillBeCreated");
                    AuditString.AppendLine($"1.1CustomOrderService.User.{Input.Email}.WillBeCreated");
                    var password = PasswordGenerator.GenerateRandomPassword();
                    var userCreationResult = await clientServices.CreateNewUserWithPassword(user, password);
                    if (userCreationResult.ResultValue == Fundamentals.ResultValue.Success)
                    {
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
                        await Task.WhenAll(new List<Task>() { confirmEmailTask, PassworkEmailTask });
                    }
                }
                var enabledService = enableService.GetEnabledService(InputEnableService.ServiceDetail.Name, InputEnableService.Location.City);
                enabledService.CostToConsultant = InputEnableService.CostToConsultant;
                enabledService.CostToCustomer = InputEnableService.CostToCustomer;
                await CreateFreshOrder(enabledService, Input.Email, Input.LinkOrderId);
            }
            catch (Exception error)
            {
                logger.LogCritical(LogEvents.CustomOrderError, $"Unknow Error in CustomOrder {error.Message}");
                AuditString.AppendLine($"#.Unknow Error in CustomOrder { error.Message}");
            }
            finally {
                OrderAudit audit = new OrderAudit();
                audit.Email = Input.Email ?? User.Identity.Name;
                audit.DateOfOrder = DateTime.UtcNow;
                audit.OrderReceipt = Receipt;
                audit.History = AuditString.ToString().Split('\n').ToList();
                await orderAuditService.AddAudit(audit);
            }


            return RedirectToPage("/Order/CustomOrderList");
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

        private bool VerifyExistingUserMismatch(Clientele user)
        {
            var existingUserByPhone = userManager.Users.Where(item => item.PhoneNumber == user.PhoneNumber).FirstOrDefault();
            var existingUserByEmail = userManager.Users.Where(item => item.Email.ToLower() == Input.Email.ToLower()).FirstOrDefault();
            bool returnVal = false;
            if (existingUserByPhone != null || existingUserByEmail != null)
            {

                if (existingUserByEmail != null && string.Compare(existingUserByPhone.PhoneNumber, user.PhoneNumber, true) != 0)
                {
                    ModelState.AddModelError(string.Empty, $"User with Email {existingUserByEmail.Email} exists in system with Phone {existingUserByPhone.PhoneNumber}");
                    returnVal = true;
                }
                if (existingUserByPhone != null && string.Compare(existingUserByPhone.Email, user.Email, true) != 0)
                {
                    ModelState.AddModelError(string.Empty, $"User with Phone {existingUserByPhone.PhoneNumber} exists in system with  Email {existingUserByPhone.Email}");
                    returnVal = true;
                }

            }
            return returnVal;
        }

        public async Task CreateFreshOrder(EnabledServices CurrentOrderService, string UserEmail = "",string LinkOrderId="")
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
            var userDetails = string.IsNullOrEmpty(UserEmail) ? await clientServices.GetUserAsync(User) : await clientServices.GetByEmail(UserEmail);

            order.Receipt = ShortId.Generate(options).ToUpper();
            Receipt = order.Receipt;
            order.ClientId = userDetails.Id;
            order.OrderStatus = OrderStatus.WaitingForCustomerPayment;
            order.IsDeleted = false;
            order.CreatedBy = User.Identity.Name;
            order.LinkOrderId = string.IsNullOrEmpty(LinkOrderId) ? ObjectId.GenerateNewId() : ObjectId.Parse(LinkOrderId);
            var finalOrder = await orderService.SaveOrder(order);
            logger.LogInformation($"2.CustomOrderService.FreshOrder.OrderId.{finalOrder.ClientOrderId.ToString()}.Client.{userDetails.Id}");
            AuditString.AppendLine($"2.CustomOrderService.FreshOrder.OrderId.{finalOrder.ClientOrderId.ToString()}.Client.{userDetails.Id}");

        }
    }
}