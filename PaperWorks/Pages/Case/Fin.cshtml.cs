using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using CaseManagement;
using CaseManagementSpace;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using OrderAndPayments;
using Razorpay.Api;
using Users;

namespace PaperWorks
{
    public class FinModel : PageModel
    {
        private readonly ICasePaymentReleaseService casePaymentReleaseService;
        private readonly ICaseManagement caseManagement;
        private readonly IPaymentService paymentService;
        private readonly UserManager<Clientele> userManager;
        private readonly RoleManager<MongoRole> roleManager;
        private readonly IPaymentNudgeService nudGeService;

        [BindProperty]
        public string PaidSoFar { get; set; }
        [BindProperty]
        public string FinalizedCost { get; set; }
        [BindProperty]
        public string ServiceName { get; set; }
        [BindProperty(SupportsGet = true)]
        public string Receipt { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ConsultantEmail { get; set; }

        [BindProperty]
        public Dictionary<string, string> PAYDATAFROMRAZOR { get; set; }
        public bool IsNudgeOn { get; set; }


        [BindProperty]
        public ClientRefund InputForRefund { get; set; }

        public class Input
        {
            [Required(ErrorMessage = "Valid Comment is required")]
            [Display(Name = "Comments")]
            public string PaymentComments { get; set; }
            [Required(ErrorMessage = "Mandatory Transaction Id Or Cheque No")]
            [Display(Name = "NEFT/UPI/RTGS Transaction Id or Cheque No")]
            public string PaymentIdentifier { get; set; }
            [Required(ErrorMessage = "Payment Amount is Required")]
            [DataType(DataType.Currency, ErrorMessage = "Please enter Valid amount")]
            [Display(Name = "Amount in Numbers")]
            public double Payment { get; set; }
        }

        [BindProperty(SupportsGet = true)]
        public Input PostAPayment { get; set; }
        [BindProperty]
        public PayToConsultant FullPayInfo { get; set; }

        public ClientelePayment CustomerPayment { get; set; }


        public Payment CustomerPaymentWithRazor { get; set; }

        [BindProperty]
        public NudgeInfo NudgeInfo { get; set; }


        public FinModel(ICasePaymentReleaseService casePaymentReleaseService, ICaseManagement caseManagement, IPaymentService paymentService, UserManager<Clientele> userManager, RoleManager<MongoRole> roleManager, IPaymentNudgeService nudGeService)
        {
            this.casePaymentReleaseService = casePaymentReleaseService;
            this.caseManagement = caseManagement;
            this.paymentService = paymentService;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.nudGeService = nudGeService;
        }
        public async Task<IActionResult> OnGetAsync(string rct, string cMail = null)
        {
            var usersInRoleTask = await userManager.GetUsersInRoleAsync("Consultant");

            //By Email we can directly pay a previous consultant as well. But that consultant should still have consultant Role
            //Otherwise it will take the CurrentconsultantId from Case and make payment to it

            var caseByReceipt = caseManagement.GetCaseByReceipt(rct);

            var consultantInfo = cMail != null ? usersInRoleTask.Where(x => x.Email == cMail).FirstOrDefault() : usersInRoleTask.Where(x => x.Id == caseByReceipt.Result.CurrentConsultantId).FirstOrDefault();


            FullPayInfo = consultantInfo != null ? await casePaymentReleaseService.GetPaymentsForCase(caseByReceipt.Result.CaseId.ToString(), consultantInfo.Id.ToString()) : null;


            CustomerPayment = await paymentService.GetPaymentByCaseId(caseByReceipt.Result.CaseId.ToString());
            try
            {
                RazorpayClient client = new RazorpayClient("rzp_test_ju6u0OTTuolb5J", "mUb1k41FXOvU9qrCFAyqQAY4");
                CustomerPaymentWithRazor = client.Payment.Fetch(CustomerPayment.GateWayDetails.PaymentGateWay_PayId);
                //CustomerPaymentWithRazor = new Razorpay.Api.Payment(CustomerPayment.GateWayDetails.PaymentGateWay_PayId).Fetch(CustomerPayment.GateWayDetails.PaymentGateWay_PayId);

            }
            catch (Exception error)
            {
            }
            PAYDATAFROMRAZOR = new Dictionary<string, string>();

            foreach (var child in CustomerPaymentWithRazor.Attributes)
            {
                if (child.Type == JTokenType.Property)
                {
                    var property = child as Newtonsoft.Json.Linq.JProperty;
                    PAYDATAFROMRAZOR.Add(property.Name, property.Value.ToString());
                }
            }

            ServiceName = caseByReceipt.Result.Order.ServiceName;
            FinalizedCost = FullPayInfo?.FinalizedCost.ToString() ?? "###";
            PaidSoFar = FullPayInfo?.PaymentReleased.ToString() ?? "###";
            Receipt = rct;
            ConsultantEmail = consultantInfo?.Email ?? "#####";

            NudgeInfo = await nudGeService.GetNudge(rct);
            IsNudgeOn = NudgeInfo == null ? false : true;

            ModelState.Clear();

            return Page();
        }

        public async Task<PartialViewResult> OnPostReleasePaymentAsync()
        {
            var usersInRoleTask = await userManager.GetUsersInRoleAsync("Consultant");
            var consultant = usersInRoleTask.Where(x => x.Email == ConsultantEmail).FirstOrDefault();

            if (consultant == null) return Partial("_PayUpdates", null);//No Consultant attached
            

            var caseByReceipt = await caseManagement.GetCaseByReceipt(Receipt);

            var paymentsInfo = await casePaymentReleaseService.ReleasePayment(caseByReceipt.CaseId.ToString(), consultant.Id.ToString(), new PaymentReleaseInfo() { Payment = PostAPayment.Payment, PaymentComments = PostAPayment.PaymentComments, PaymentIdentifier = PostAPayment.PaymentIdentifier });
            FullPayInfo = await casePaymentReleaseService.GetPaymentsForCase(caseByReceipt.CaseId.ToString(), consultant.Id.ToString());

            NudgeInfo = await nudGeService.GetNudge(Receipt);
            IsNudgeOn = NudgeInfo == null ? false : true;
            if (IsNudgeOn)
            {
                nudGeService.EndANudge(User.Identity.Name, Receipt);
            }
            PaidSoFar = FullPayInfo?.PaymentReleased.ToString() ?? "###";
            
            return Partial("_PayUpdates", FullPayInfo);

        }

        public async Task<IActionResult> OnPostRefundPaymentAsync()
        {
            try
            {
                var caseByReceipt = caseManagement.GetCaseByReceipt(Receipt);
                var customerPayment =  await paymentService.GetPaymentByCaseId(caseByReceipt.Result.CaseId.ToString());

                CustomerPayment = await paymentService.GetPaymentByCaseId(caseByReceipt.Result.CaseId.ToString());

                RazorpayClient client = new RazorpayClient("rzp_test_ju6u0OTTuolb5J", "mUb1k41FXOvU9qrCFAyqQAY4");
                CustomerPaymentWithRazor = client.Payment.Fetch(CustomerPayment.GateWayDetails.PaymentGateWay_PayId);
                //CustomerPaymentWithRazor = new Razorpay.Api.Payment(CustomerPayment.GateWayDetails.PaymentGateWay_PayId).Fetch(CustomerPayment.GateWayDetails.PaymentGateWay_PayId);
                if (customerPayment.FinalAmount < InputForRefund.RefundAmount)
                {
                    ModelState.AddModelError(string.Empty, $"Amount can not be greater than customer payment amount {InputForRefund.RefundAmount}");
                }
                Dictionary<string, object> data = new Dictionary<string, object>();
                data.Add("amount", (InputForRefund.RefundAmount*100).ToString());
                return RedirectToPage("/Case/CustomerRefund", new { receipt = Receipt });

                
            }
            catch (Exception error)
            {
            }
            return RedirectToPage("/Case/CaseListing");
        }
    }
}