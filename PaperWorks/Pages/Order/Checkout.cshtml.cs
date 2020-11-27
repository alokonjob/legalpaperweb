using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Address;
using CaseManagementSpace;
using Emailer;
using Fundamentals.Managers;
using Fundamentals.Unit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using OrderAndPayments;
using Phone;
using Razorpay.Api;
using shortid;
using shortid.Configuration;
using Tax;
using User;
using Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Fundamentals.Events;
using Audit;
using Microsoft.Extensions.Localization;

namespace PaperWorks
{
    public class CheckoutModel : PageModel
    {
        private readonly IEnabledServices enabledServicesManager;
        private readonly ITaxService taxService;
        private readonly IClienteleServices clienteleServices;

        private readonly CountryService countryService;
        private readonly IEmailer emailSender;
        private readonly IPhoneService phoneService;
        private readonly IPaymentService paymentService;
        private readonly IOrderService orderService;
        private readonly ICaseManagement caseManagement;
        private readonly IOrderAuditService orderAudit;
        private readonly ILogger<CheckoutModel> logger;
        private readonly IStringLocalizer<CheckoutModel> localizer;

        public List<SelectListItem> AvailableCountries { get; }
        public string TaxAmount { get; set; }
        public string FinalAmount { get; set; }
        public string FinalAmountForRazor { get; set; }
        public EnabledServices CurrentOrderService = null;

        [TempData]
        public string CustomerOrderId { get; set; }
        StringBuilder AuditString = new StringBuilder();

        public CheckoutModel(IEnabledServices enabledServicesManager, ITaxService taxService,IClienteleServices clienteleServices,
            CountryService countryService, IEmailer emailSender,IPhoneService phoneService,
            IPaymentService paymentService,IOrderService orderService,ICaseManagement caseManagement,IOrderAuditService orderAudit , ILogger<CheckoutModel> logger, IStringLocalizer<CheckoutModel> localizer)
        {
            this.enabledServicesManager = enabledServicesManager;
            this.taxService = taxService;
            this.clienteleServices = clienteleServices;


            this.countryService = countryService;
            this.emailSender = emailSender;
            this.phoneService = phoneService;
            this.paymentService = paymentService;
            this.orderService = orderService;
            this.caseManagement = caseManagement;
            this.orderAudit = orderAudit;
            this.logger = logger;
            this.localizer = localizer;
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
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
            [Display(Name = "Phone number country")]
            public string PhoneNumberCountryCode { get; set; }
        }

        public string orderId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            double finalAmount = 0.0;

            var orderReceipt = HttpContext.Session.GetString("OrderReceipt");
            var clienteleOrder = await orderService.GetOrderByReceipt(orderReceipt);
            if (!string.IsNullOrEmpty(orderReceipt))
            {
                CurrentOrderService = enabledServicesManager.GetEnabledServiceById(clienteleOrder.CustomerRequirementDetail.EnableId);
                double currentApplicableTax = taxService.GetTaxAmount(CurrentOrderService.Location.State, CurrentOrderService.ServiceDetail.Name, CurrentOrderService.CostToCustomer);
                finalAmount = currentApplicableTax + CurrentOrderService.CostToCustomer;
                TaxAmount = currentApplicableTax.ToString("#,##0.00");
                FinalAmount = (finalAmount).ToString("#,##0.00");
                FinalAmountForRazor = (finalAmount*100).ToString();
            }

            //Prepare Razor Pay Order Input data

            orderId = PrepareRazorPe(clienteleOrder);

            if (clienteleServices.IsSignedIn(User))
            {
                var signedInUser = await clienteleServices.GetUserAsync(User);
                 var phoneNumber = signedInUser.PhoneNumber != null ? signedInUser.PhoneNumber.Substring(signedInUser.PhoneNumber.Length - 10) : "";
                Input = new InputModel() { Name = signedInUser.UserName, Email = signedInUser.Email, PhoneNumber = phoneNumber };
            }
            return Page();
        }

        private string PrepareRazorPe(ClienteleOrder order)
        {
            Dictionary<string, object> razorPayInput = new Dictionary<string, object>();
            razorPayInput.Add("amount", FinalAmountForRazor); // this amount should be same as transaction amount
            razorPayInput.Add("currency", "INR");
            razorPayInput.Add("receipt", order.Receipt);
            razorPayInput.Add("payment_capture", 1);

            string key = "rzp_test_ju6u0OTTuolb5J";
            string secret = "mUb1k41FXOvU9qrCFAyqQAY4";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            RazorpayClient client = new RazorpayClient(key, secret);

            Razorpay.Api.Order razorPeOrder = client.Order.Create(razorPayInput);
            return razorPeOrder["id"].ToString();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //If User is not already signed in then create a login
            bool OrderFullyCompleted = false; 
            try
            {
                
                if (clienteleServices.IsSignedIn(User) == false)
                {
                    return RedirectToPage("./OrderConfirmation");
                }
                else if (clienteleServices.IsSignedIn(User))
                {
                    var orderReceipt = HttpContext.Session.GetString("OrderReceipt");
                    var clienteleOrder = await orderService.GetOrderByReceipt(orderReceipt);


                    CurrentOrderService = enabledServicesManager.GetEnabledServiceById(clienteleOrder.CustomerRequirementDetail.EnableId);

                    //Prepare Pyment Object

                    var clientePayment = PrepareClientelePayment(Request.Form["pid"], Request.Form["oid"], Request.Form["sid"], Request.Form["cid"]);

                    /*save Clientele Payment in our Database*/
                    var payId = paymentService.SavePayment(clientePayment);
                    logger.LogInformation(LogEvents.SaveClientPayment, $"Payment.Save.Success.ForOrderReceipt.{clienteleOrder.Receipt}");
                    AuditString.AppendLine($"Checkout.{LogEvents.SaveClientPayment}.{Input.Email}.Success");

                    //verify payment with Razor
                    paymentService.VerifyPayment(clientePayment.GateWayDetails);

                    logger.LogInformation(LogEvents.VerificationWithGateWay, $"Payment.Verification.Success.ForOrderReceipt.{clienteleOrder.Receipt}");
                    AuditString.AppendLine($"Checkout.{LogEvents.VerificationWithGateWay}.{Input.Email}.Success");
                    clienteleOrder.ClientelePaymentId = payId.Result;

                    var finalOrder = await orderService.AddPaymentToOrder(clienteleOrder.ClientOrderId.ToString(), payId.Result.ToString());
                    logger.LogInformation(LogEvents.PaymentSavedInOrder, $"Payment.OrderUpdated.Success.ForOrderReceipt.{clienteleOrder.Receipt}");
                    AuditString.AppendLine($"Checkout.{LogEvents.PaymentSavedInOrder}.{Input.Email}.Success");

                    


                    CustomerOrderId = finalOrder.ClientOrderId.ToString();

                    Case clientCase = new Case();
                    var userDetails = await clienteleServices.GetUserAsync(User);

                    clientCase.Order = new AbridgedOrder()
                    {
                        OrderId = clienteleOrder.ClientOrderId,
                        ServiceName = finalOrder.CustomerRequirementDetail.ServiceDetail.Name,
                        City = finalOrder.CustomerRequirementDetail.Location.City,
                        CustomerEmail = userDetails.Email,
                        CustomerPhone = userDetails.PhoneNumber,
                        CostToCustomer = finalOrder.CustomerRequirementDetail.CostToCustomer.ToString(),
                        CustomerName = userDetails.FullName,
                        Receipt = clienteleOrder.Receipt

                    };
                    clientCase.CreatedDate = clienteleOrder.OrderPlacedOn;
                    clientCase.PreviousConsultantId = new List<MongoDB.Bson.ObjectId>();

                    var confirmcallbackUrl = Url.Page(
                        "/Order/OrderList",
                        pageHandler: null,
                        values: new {receipt  = clienteleOrder.Receipt, returnUrl = Url.Content("~/")},
                        protocol: Request.Scheme);

                    Dictionary<string, string> itemDictionary = new Dictionary<string, string>();
                    itemDictionary.Add("##SERVICE",localizer[clientCase.Order.ServiceName]);
                    itemDictionary.Add("##ORDERNO", clientCase.Order.Receipt);
                    itemDictionary.Add("##CITY", clientCase.Order.City.ToUpper());
                    itemDictionary.Add("##NAME", clientCase.Order.CustomerName);
                    itemDictionary.Add("##PHONE", clientCase.Order.CustomerPhone);
                    itemDictionary.Add("##URL", HtmlEncoder.Default.Encode(confirmcallbackUrl));

                    var caseId = await caseManagement.GenerateCase(clientCase, itemDictionary);

                    await paymentService.UpdatePayment(payId.Result, finalOrder.ClientOrderId, caseId, paymentService.GetPaymentStatusFromPaymentGateWay(clientePayment));
                    logger.LogInformation(LogEvents.OrderSavedInPayment, $"Payment.PaymentUpdated.Success.ForOrderReceipt.{clienteleOrder.Receipt}");
                    AuditString.AppendLine($"Checkout.{LogEvents.OrderSavedInPayment}.{Input.Email}.Success");

                    await orderService.AddCaseToOrder(clienteleOrder.ClientOrderId, caseId);
                    logger.LogInformation(LogEvents.AddCaseToOrder, $"Order.AddedToCase.Success.ForOrderReceipt.{clienteleOrder.Receipt}");
                    AuditString.AppendLine($"Checkout.{LogEvents.AddCaseToOrder}.{Input.Email}.Success");

                }
                //If user is already signed in but no phone. Ask to Enter Phone.
                //If User is already signed in and there phone/email verification and email verifi
                OrderFullyCompleted = true;
                return RedirectToPage("./OrderConfirmation");
            }
            catch (Exception error)
            {
                logger.LogCritical(LogEvents.CheckoutError, error,$"Checkout.Error.{error.Message}");
                AuditString.AppendLine($"Checkout.{LogEvents.CheckoutError}.{error.Message}");
            }
            finally 
            {
                OrderFullyCompleted = true;
                await orderAudit.UpdateAudit(HttpContext.Session.GetString("OrderReceipt"), AuditString.ToString().Split('\n').ToList(), OrderFullyCompleted);
            }
            return Page();
        }

        public ClientelePayment PrepareClientelePayment(string paymentId, string orderId, string sigId,string cid)
        {
            ClientelePayment paymentDone = new ClientelePayment();
            paymentDone.GateWayDetails = new RazorPePaymentDetails()
            {
                PaymentGateWay_OrderId = orderId,
                PaymentGateWay_PayId = paymentId,
                PaymentGateWay_Signature = sigId
            };

            paymentDone.FinalAmount = Convert.ToDouble(cid);
            paymentDone.PaymentDate = DateTime.UtcNow;
            paymentDone.RefundDetails = new List<ClientRefund>();
            return paymentDone;
        }

    }
}