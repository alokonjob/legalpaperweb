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
    public class CheckoutModel : PageModel
    {
        private readonly IEnabledServices enabledServicesManager;
        private readonly ITaxService taxService;
        private readonly UserManager<Clientele> userManager;
        private readonly SignInManager<Clientele> signInManager;
        private readonly CountryService countryService;
        private readonly IEmailer emailSender;
        private readonly IPhoneService phoneService;
        private readonly IPaymentService paymentService;
        private readonly IOrderService orderService;
        private readonly ICaseManagement caseManagement;

        public List<SelectListItem> AvailableCountries { get; }
        public string TaxAmount { get; set; }
        public string FinalAmount { get; set; }
        public EnabledServices CurrentOrderService = null;

        [TempData]
        public string CustomerOrderId { get; set; }

        public CheckoutModel(IEnabledServices enabledServicesManager, ITaxService taxService, UserManager<Clientele> userManager,
            SignInManager<Clientele> signInManager,
            CountryService countryService, IEmailer emailSender,IPhoneService phoneService,IPaymentService paymentService,IOrderService orderService,ICaseManagement caseManagement)
        {
            this.enabledServicesManager = enabledServicesManager;
            this.taxService = taxService;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.countryService = countryService;
            this.emailSender = emailSender;
            this.phoneService = phoneService;
            this.paymentService = paymentService;
            this.orderService = orderService;
            this.caseManagement = caseManagement;
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

        public void OnGetAsync()
        {
            var requiredServiceId= HttpContext.Session.GetString("DataBaseId");
            if (!string.IsNullOrEmpty(requiredServiceId))
            {
                CurrentOrderService = enabledServicesManager.GetEnabledServiceById(requiredServiceId);
                double currentApplicableTax = taxService.GetTaxAmount(CurrentOrderService.Location.State, CurrentOrderService.ServiceDetail.Name, CurrentOrderService.CostToCustomer);
                double finalAmount = currentApplicableTax + CurrentOrderService.CostToCustomer;
                TaxAmount = currentApplicableTax.ToString();
                FinalAmount = finalAmount.ToString();
            }
            Dictionary<string, object> input = new Dictionary<string, object>();
            input.Add("amount", FinalAmount); // this amount should be same as transaction amount
            input.Add("currency", "INR");
            input.Add("receipt", "12121");
            input.Add("payment_capture", 1);

            string key = "rzp_test_ju6u0OTTuolb5J";
            string secret = "mUb1k41FXOvU9qrCFAyqQAY4";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            RazorpayClient client = new RazorpayClient(key, secret);

            Razorpay.Api.Order order = client.Order.Create(input);
            orderId = order["id"].ToString();
            if (signInManager.IsSignedIn(User))
            {
                var signedInUser = userManager.GetUserAsync(User).Result;
                var phoneNumber = signedInUser.PhoneNumber != null ? signedInUser.PhoneNumber.Substring(signedInUser.PhoneNumber.Length - 10) : "";
                Input = new InputModel() { Name = signedInUser.UserName, Email = signedInUser.Email, PhoneNumber = phoneNumber };
            }
        }

        //TODO : Add Log and Make Truly Asunc
        //Researh needed because in async Post Calls Redirection was giving problems
        public IActionResult OnPostAsync()
        {
            //If User is not already signed in then create a login
            if (signInManager.IsSignedIn(User) == false)
            {
                 
            }
            else if (signInManager.IsSignedIn(User))
            {
                var requiredServiceId = HttpContext.Session.GetString("DataBaseId");
                CurrentOrderService = enabledServicesManager.GetEnabledServiceById(requiredServiceId);
                if (string.IsNullOrEmpty(requiredServiceId)) { 
                //return ErrorPage
                }
                string paymentId = Request.Form["pid"];
                string orderId = Request.Form["oid"];
                string sigId = Request.Form["sid"];
                string cid = Request.Form["cid"];

                //Prepare Pyment Object

                ClientelePayment paymentDone = new ClientelePayment();
                paymentDone.GateWayDetails = new RazorPePaymentDetails()
                {
                    PaymentGateWay_OrderId = orderId,
                    PaymentGateWay_PayId = paymentId,
                    PaymentGateWay_Signature = sigId
                };
                paymentDone.FinalAmount =Convert.ToDouble(cid);
                paymentDone.PaymentDate = DateTime.UtcNow;
                //savePayment
                var payId = paymentService.SavePayment(paymentDone).Result;
                
                //verify payment
                paymentService.VerifyPayment(paymentDone.GateWayDetails);

                //create an order
                ClienteleOrder order = new ClienteleOrder();
                order.ClientelePaymentId = payId;
                order.CustomerRequirementDetail = CurrentOrderService;
                order.OrderPlacedOn = DateTime.UtcNow;
                var options = new GenerationOptions
                {
                    UseNumbers = true
                };
                var userDetails = userManager.GetUserAsync(User).Result;

                order.Receipt = ShortId.Generate(options);
                order.ClientId = userDetails.Id;
                var finalOrder = orderService.SaveOrder(order).Result;

                //Update Payment Status
                var payInfoAtRazor = new Razorpay.Api.Payment((string)paymentId).Fetch(paymentId);
                paymentService.UpdatePayment(payId, finalOrder.ClientOrderId, "captured");
                CustomerOrderId = finalOrder.Receipt;

                //generate a case //should we also have a case id in order so that we can filter out order without cases if any
                Case clientCase = new Case();
                clientCase.CaseManagerId = userDetails.Id;
                clientCase.Order = new AbridgedOrder()
                {
                    OrderId = order.ClientOrderId,
                    ServiceName = finalOrder.CustomerRequirementDetail.ServiceDetail.Name,
                    City = finalOrder.CustomerRequirementDetail.Location.City,
                    CustomerEmail = userDetails.Email,
                    CustomerPhone = userDetails.PhoneNumber,
                    //ConsultantEmail = userDetails.Email,
                    //ConsultantPhone = userDetails.PhoneNumber,
                    CostToCustomer = finalOrder.CustomerRequirementDetail.CostToCustomer.ToString()

                };
                var caseId = caseManagement.GenerateCase(clientCase).Result;

                var updatedOrder = orderService.AddCaseToOrder(order.ClientOrderId,caseId).Result;

            }
            //If user is already signed in but no phone. Ask to Enter Phone.
            //If User is already signed in and there phone/email verification and email verifi
            return RedirectToPage("./OrderConfirmation");
        }
    }
}