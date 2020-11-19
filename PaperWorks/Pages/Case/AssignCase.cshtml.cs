using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CaseManagement;
using CaseManagementSpace;
using Consultant;
using Fundamentals.Extensions;
using Fundamentals.Unit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using OrderAndPayments;
using User;
using Users;

namespace PaperWorks
{
    public class AssignCaseModel : PageModel
    {
        private readonly ICaseManagement caseManagement;
        private readonly IConsultantCareerManagement consultantManagement;
        private readonly IClienteleServices clientServices;
        private readonly ICasePaymentReleaseService casePaymentReleaseService;
        private readonly ILogger<AssignCaseModel> logger;

        [Required]
        [BindProperty(SupportsGet =true)]
        public string SeletedEmail{ get; set; }

        [BindProperty(SupportsGet = true)]
        public string FinalizedCost { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Receipt { get; set; }
        public List<UserUIInfo> FullConsultantDetails { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReasonOfAssignment { get; set; }
        public AssignCaseModel(ICaseManagement caseManagement, IConsultantCareerManagement consultantManagement, IClienteleServices clientServices,
            ICasePaymentReleaseService casePaymentReleaseService , ILogger<AssignCaseModel> logger)
        {
            this.caseManagement = caseManagement;
            this.consultantManagement = consultantManagement;
            this.clientServices = clientServices;
            this.casePaymentReleaseService = casePaymentReleaseService;
            this.logger = logger;
        }
        public async Task<IActionResult> OnGetAsync(string rct)
        {
            var cases = await caseManagement.GetCaseByReceipt(rct);
            Receipt  = rct;
            var consultants = await consultantManagement.GetConsultantForEnabledService(cases.Order.ServiceName, cases.Order.City);
            var userIds = consultants.Select(x => x.ConsultantId).ToList();
            var consulTatDetails = await clientServices.GetUserByIds(userIds);
            FullConsultantDetails = consultants.Select(x=> new UserUIInfo() { ConsultantDetails = x , UserDetails = consulTatDetails.Where(y=>y.Id ==  x.ConsultantId).FirstOrDefault() })
                .ToList()
                .OrderBy(x=>  x.ConsultantDetails.CurrentService.Fee )
                .ThenByDescending(x => x.ConsultantDetails.RatingsValue).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var userDetails = await clientServices.GetByEmail(SeletedEmail);
                var existingCaseDetail = await caseManagement.GetCaseByReceipt(Receipt);
                Case caseToUpdate = new Case();
                caseToUpdate.Order = new AbridgedOrder();
                caseToUpdate.Order.ConsultantEmail = SeletedEmail;
                caseToUpdate.Order.ConsultantPhone = userDetails.PhoneNumber;
                caseToUpdate.CurrentConsultantId = userDetails.Id;
                caseToUpdate.CaseId = existingCaseDetail.CaseId;
                if (existingCaseDetail.CurrentConsultantId.ToString() != "000000000000000000000000")
                {
                    caseToUpdate.PreviousConsultantId = new List<MongoDB.Bson.ObjectId>() { existingCaseDetail.CurrentConsultantId };
                }
                else
                {
                    caseToUpdate.PreviousConsultantId = new List<MongoDB.Bson.ObjectId>();
                }
                var casepaymentInfo = casePaymentReleaseService.SetFinalizedCost(caseToUpdate.CaseId.ToString(), caseToUpdate.CurrentConsultantId.ToString(), Convert.ToDouble(FinalizedCost));

                var user = caseManagement.UpdateConsultant(caseToUpdate);

                await Task.WhenAll(new List<Task>() { casepaymentInfo, user });
                if (user != null)
                {
                    return RedirectToPage("./CaseDetail", new { rct = Receipt });
                }
                return Page();
            }
            catch (Exception error)
            {
                logger.LogCritical(error, error.Message);
                throw;
            }
        }

        public class UserUIInfo
        { 
            public ConsultantCareer ConsultantDetails { get; set; }
            public Clientele UserDetails { get; set; }
            public bool Selected { get; set; }
        }
    }
}