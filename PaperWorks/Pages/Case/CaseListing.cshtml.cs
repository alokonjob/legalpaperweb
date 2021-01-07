using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaseManagement;
using CaseManagementSpace;
using Fundamentals.Extensions;
using Fundamentals.Unit;
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
        private readonly INudgeService nudgeService;


        public List<CaseFullInfo> FullCaseInfo { get; set; }
        public List<Case> CaseList { get; set; }
        [BindProperty]
        public DateTime DateTime { get; set; }
        public CaseListingModel(UserManager<Clientele> userManager,
            SignInManager<Clientele> signInManager, ICaseManagement caseManagement, ICasePaymentReleaseService paymentService, INudgeService nudgeService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.caseManagement = caseManagement;
            this.paymentService = paymentService;
            this.nudgeService = nudgeService;
        }

        
        
        [BindProperty(SupportsGet = true)]
        public Filters InputForFilters { get; set; }

        public async Task<IActionResult> OnGet(string userEmail = null)
        {
            InputForFilters = new Filters() {Receipt =string.Empty, ServiceType = EnableServiceType.None,CaseStatus= CaseStatus.None, FromDate= DateDefaults.GetDefaultDate(), ToDate = DateDefaults.GetDefaultDate() };
            if (User.IsFinanceUser() || User.IsFounder())
            {
                CaseList = await caseManagement.GetAllCases(InputForFilters);
            }
            else if (User.IsCaseManager())
            {
                var loginUUser = await userManager.GetUserAsync(User);
                CaseList = await (caseManagement as ICaseManagerCaseManagement).GetAllCasesOfCaseManager(loginUUser.Email, InputForFilters);
            }
            //else if (User.IsConsultant())
            //{
            //    var loginUUser = await userManager.GetUserAsync(User);
            //    CaseList = await (caseManagement as IConsultantCaseManagement).GetAllCasesOfConsultant(loginUUser.Email);
            //}
            var payments = await paymentService.GetPaymentsForCases(CaseList.Select(x => x.CaseId).ToList());


            FullCaseInfo = CaseList.Select(x => new CaseFullInfo()
            {
                ClientCase = x,
                Payment = payments.Where(y => y.CaseId == x.CaseId).FirstOrDefault(),
                Nudge = nudgeService.IsNudgeOn(x.Order.Receipt).Result.ToString()
            }).ToList();
            return Page();
        }

        public async Task<PartialViewResult> OnPostFilterCaseAsync()
        {
            if (User.IsFinanceUser() || User.IsFounder())
            {
                CaseList = await caseManagement.GetAllCases(InputForFilters);
            }
            else if (User.IsCaseManager())
            {
                var loginUUser = await userManager.GetUserAsync(User);
                CaseList = await (caseManagement as ICaseManagerCaseManagement).GetAllCasesOfCaseManager(loginUUser.Email, InputForFilters);
            }
            //else if (User.IsConsultant())
            //{
            //    var loginUUser = await userManager.GetUserAsync(User);
            //    CaseList = await (caseManagement as IConsultantCaseManagement).GetAllCasesOfConsultant(loginUUser.Email);
            //}
            var payments = await paymentService.GetPaymentsForCases(CaseList.Select(x => x.CaseId).ToList());


            FullCaseInfo = CaseList.Select(x => new CaseFullInfo()
            {
                ClientCase = x,
                Payment = payments.Where(y => y.CaseId == x.CaseId).FirstOrDefault(),
                Nudge = nudgeService.IsNudgeOn(x.Order.Receipt).Result.ToString()
            }).ToList();
            return Partial("_CaseList", FullCaseInfo);
        }
    }

    public class CaseFullInfo
    {
        private NudgeType NudgeVal;

        public Case ClientCase { get; set; }
        public PayToConsultant Payment { get; set; }
        public string Nudge { get { return NudgeVal.GetDescription(); } set { Enum.TryParse<NudgeType>(value, out NudgeVal); } }
    }

    
}