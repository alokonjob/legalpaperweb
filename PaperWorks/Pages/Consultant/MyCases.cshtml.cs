using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaseManagementSpace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Users;

namespace PaperWorks
{
    [Authorize(Policy = "RequireConsultantRole")]
    public class MyCasesModel : PageModel
    {

        private readonly ICaseManagement caseManagement;

        public List<Case> CaseList { get; set; }
        public MyCasesModel(ICaseManagement caseManagement)
        {
            this.caseManagement = caseManagement;
        }
        public async Task<IActionResult> OnGet()
        {
            CaseList = await caseManagement.GetAllCasesOfConsultant(User.Identity.Name);
            return Page();
        }
    }
}