using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaseManagement;
using CaseManagementSpace;
using Fundamentals.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Users;

namespace PaperWorks
{
    [Authorize(Policy = "RequireConsultantRole")]
    public class MyCasesModel : PageModel
    {

        private readonly ICaseManagement caseManagement;
        private readonly ICasePaymentReleaseService paymentService;
        private readonly ILogger<MyCasesModel> logger;

        public List<CaseFullInfo> FullCaseInfo { get; set; }
        public List<Case> CaseList { get; set; }
        public MyCasesModel(ICaseManagement caseManagement, ICasePaymentReleaseService paymentService, ILogger<MyCasesModel> logger)
        {
            this.caseManagement = caseManagement;
            this.paymentService = paymentService;
            this.logger = logger;
        }
        public async Task<IActionResult> OnGet()
        {
            try
            {
                CaseList = await caseManagement.GetAllCasesOfConsultant(User.Identity.Name);
                var payments = await paymentService.GetPaymentsForCases(CaseList.Select(x => x.CaseId).ToList());
                FullCaseInfo = CaseList.Select(x => new CaseFullInfo()
                {
                    ClientCase = x,
                    Payment = payments.Where(y => y.CaseId == x.CaseId).FirstOrDefault(),
                }).ToList();

            }
            catch (Exception error)
            {
                logger.LogCritical(LogEvents.ConsultantListError, error.Message);
            }
            return Page();
        }
    }
}