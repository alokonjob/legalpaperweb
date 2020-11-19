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
using Users;

namespace PaperWorks
{
    public class FinModel : PageModel
    {
        private readonly ICasePaymentReleaseService casePaymentReleaseService;
        private readonly ICaseManagement caseManagement;
        private readonly UserManager<Clientele> userManager;
        private readonly RoleManager<MongoRole> roleManager;
        private readonly IPaymentNudgeService nudGeService;

        [BindProperty]
        public string PaidSoFar { get; set; }
        [BindProperty]
        public string FinalizedCost { get; set; }
        [BindProperty]
        public string ServiceName { get; set; }
        [BindProperty(SupportsGet =true)]
        public string Receipt { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ConsultantEmail { get; set; }

        public bool IsNudgeOn { get; set; }

        public class Input
        {
            [Required(ErrorMessage = "Valid Comment is required")]
            [Display(Name = "Comments")]
            public string PaymentComments { get; set; }
            [Required(ErrorMessage = "Mandatory Transaction Id Or Cheque No")]
            [Display(Name = "NEFT/UPI/RTGS Transaction Id or Cheque No")] 
            public string PaymentIdentifier { get; set; }
            [Required(ErrorMessage = "Payment Amount is Required")]
            [DataType(DataType.Currency,ErrorMessage ="Please enter Valid amount")]
            [Display(Name = "Amount in Numbers")] 
            public double Payment { get; set; }
        }

        [BindProperty(SupportsGet =true)] 
        public Input PostAPayment { get; set; }
        [BindProperty]
        public PayToConsultant FullPayInfo { get; set; }

        [BindProperty]
        public NudgeInfo NudgeInfo { get; set; }


        public FinModel(ICasePaymentReleaseService casePaymentReleaseService, ICaseManagement caseManagement, UserManager<Clientele> userManager,RoleManager<MongoRole> roleManager,IPaymentNudgeService nudGeService)
        {
            this.casePaymentReleaseService = casePaymentReleaseService;
            this.caseManagement = caseManagement;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.nudGeService = nudGeService;
        }
        public async Task<IActionResult> OnGetAsync(string rct, string cMail= null)
        {
            var usersInRoleTask =  await userManager.GetUsersInRoleAsync("Consultant");
            
            //By Email we can directly pay a previous consultant as well. But that consultant should still have consultant Role
            //Otherwise it will take the CurrentconsultantId from Case and make payment to it
            
            var caseByReceipt = caseManagement.GetCaseByReceipt(rct);
            
            var consultantInfo = cMail != null ? usersInRoleTask.Where(x => x.Email == cMail).FirstOrDefault(): usersInRoleTask.Where(x => x.Id == caseByReceipt.Result.CurrentConsultantId).FirstOrDefault();


            FullPayInfo = await casePaymentReleaseService.GetPaymentsForCase(caseByReceipt.Result.CaseId.ToString(), consultantInfo.Id.ToString());


            ServiceName = caseByReceipt.Result.Order.ServiceName;
            FinalizedCost = FullPayInfo.FinalizedCost.ToString();
            PaidSoFar = FullPayInfo.PaymentReleased.ToString();
            Receipt = rct;
            ConsultantEmail = consultantInfo.Email;

            NudgeInfo =  await nudGeService.GetNudge(rct);
            IsNudgeOn = NudgeInfo == null ? false : true;

            ModelState.Clear();

            return Page();
        }

        public async Task<IActionResult> OnPostReleasePaymentAsync()
        {
            var usersInRoleTask = await userManager.GetUsersInRoleAsync("Consultant");
            var consultant = usersInRoleTask.Where(x => x.Email == ConsultantEmail).FirstOrDefault();

            var caseByReceipt = await caseManagement.GetCaseByReceipt(Receipt);

            var paymentsInfo = await casePaymentReleaseService.ReleasePayment(caseByReceipt.CaseId.ToString(), consultant.Id.ToString(), new PaymentReleaseInfo(){ Payment = PostAPayment.Payment,PaymentComments = PostAPayment.PaymentComments, PaymentIdentifier = PostAPayment.PaymentIdentifier});

            NudgeInfo = await nudGeService.GetNudge(Receipt);
            if (NudgeInfo != null)
            {
                nudGeService.EndANudge(User.Identity.Name, Receipt);
            }
            //var caseById = await caseManagement.GetCaseById(Receipt);

            FullPayInfo = await casePaymentReleaseService.GetPaymentsForCase(caseByReceipt.CaseId.ToString(), consultant.Id.ToString());
            ServiceName = caseByReceipt.Order.ServiceName;
            FinalizedCost = FullPayInfo.FinalizedCost.ToString();
            PaidSoFar = FullPayInfo.PaymentReleased.ToString();
            Receipt = Receipt;
            ConsultantEmail = ConsultantEmail;
            return RedirectToPage("/Case/CaseListing");
        }
    }
}