using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaseManagementSpace;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Users;

namespace PaperWorks
{
    public class CaseListingModel : PageModel
    {
        private readonly UserManager<Clientele> userManager;
        private readonly SignInManager<Clientele> signInManager;
        private readonly ICaseManagement caseManagement;

        public List<Case> CaseList { get; set; }
        public CaseListingModel(UserManager<Clientele> userManager,
            SignInManager<Clientele> signInManager,ICaseManagement caseManagement)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.caseManagement = caseManagement;
        }
        public void OnGet()
        {
            CaseList = caseManagement.GetAllCases().Result;
        }
    }
}