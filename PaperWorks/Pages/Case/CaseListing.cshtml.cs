using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaseManagement;
using CaseManagementSpace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using User;
using Users;

namespace PaperWorks
{
    [Authorize(Policy = "AccessCasesPolicy")]
    public class CaseListingModel : PageModel
    {
        private readonly UserManager<Clientele> userManager;
        private readonly SignInManager<Clientele> signInManager;
        private readonly ICaseManagement caseManagement;
        private readonly ICasePaymentReleaseService paymentService;
        private readonly IPaymentNudgeService nudgeService;

        public class CaseFullInfo
        {
            public Case ClientCase { get; set; }
            public PayToConsultant Payment { get; set; }
            public string Nudge { get; set; } 
        }
        public List<CaseFullInfo> FullCaseInfo{get;set;}
        public List<Case> CaseList { get; set; }

        public CaseListingModel(UserManager<Clientele> userManager,
            SignInManager<Clientele> signInManager,ICaseManagement caseManagement,ICasePaymentReleaseService paymentService, IPaymentNudgeService nudgeService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.caseManagement = caseManagement;
            this.paymentService = paymentService;
            this.nudgeService = nudgeService;
        }
        public async Task<IActionResult> OnGet(string userEmail = null)
        {
            
            if (User.IsFinanceUser() || User.IsFounder())
            {
                CaseList = await caseManagement.GetAllCases();
            }
            else if (User.IsCaseManager())
            {
                var loginUUser = await userManager.GetUserAsync(User);
                CaseList = await (caseManagement as ICaseManagerCaseManagement).GetAllCasesOfCaseManager(loginUUser.Email);
            }
            else if (User.IsConsultant())
            {
                var loginUUser = await userManager.GetUserAsync(User);
                CaseList = await (caseManagement as IConsultantCaseManagement).GetAllCasesOfConsultant(loginUUser.Email);
            }
            var payments = await paymentService.GetPaymentsForCases(CaseList.Select(x => x.CaseId).ToList());
            

            FullCaseInfo = CaseList.Select(x => new CaseFullInfo() { ClientCase = x, Payment = payments.Where(y => y.CaseId == x.CaseId).FirstOrDefault(), Nudge = nudgeService.IsNudgeOn(x.Order.Receipt).Result == true ? "NudgeOn" : "NudgeOff" }).ToList();
            return Page();
        }
    }
}