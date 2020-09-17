using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CaseManagementSpace;
using Consultant;
using Fundamentals.Extensions;
using Fundamentals.Unit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        [Required]
        [BindProperty(SupportsGet =true)]
        public string SeletedEmail{ get; set; }
        [BindProperty(SupportsGet = true)]
        public string CaseId { get; set; }
        public List<UserUIInfo> FullConsultantDetails { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReasonOfAssignment { get; set; }
        public AssignCaseModel(ICaseManagement caseManagement, IConsultantCareerManagement consultantManagement, IClienteleServices clientServices)
        {
            this.caseManagement = caseManagement;
            this.consultantManagement = consultantManagement;
            this.clientServices = clientServices;
        }
        public async Task<IActionResult> OnGetAsync(string caseId)
        {
            var cases = await caseManagement.GetCaseById(caseId);
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
            var userDetails = await clientServices.GetByEmail(SeletedEmail);
            
            Case caseToUpdate = new Case();
            caseToUpdate.Order = new AbridgedOrder();
            caseToUpdate.Order.ConsultantEmail = SeletedEmail;
            caseToUpdate.Order.ConsultantPhone = userDetails.PhoneNumber;
            caseToUpdate.CaseId = CaseId.ToObjectId();

            var user = await caseManagement.UpdateConsultant(caseToUpdate);
            if (user != null)
            {
                return RedirectToPage("./CaseDetail", new { caseId = CaseId });
            }
            return Page();
        }

        public class UserUIInfo
        { 
            public ConsultantCareer ConsultantDetails { get; set; }
            public Clientele UserDetails { get; set; }
            public bool Selected { get; set; }
        }
    }
}