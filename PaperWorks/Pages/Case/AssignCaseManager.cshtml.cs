using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CaseManagementSpace;
using Fundamentals.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using User;
using Users;

namespace PaperWorks
{
    public class AssignCaseManagerModel : PageModel
    {
        private readonly IClienteleServices userServices;
        private readonly IClienteleStaffServices clienteleStaffServices;
        private readonly ILogger<AssignCaseManagerModel> logger;

        public List<Clientele> CaseManagers { get; set; }

        [Required]
        [BindProperty(SupportsGet = true)]
        public string SeletedEmail { get; set; }
        public Clientele CurrentCaseManager { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Receipt { get; set; }
        public ICaseManagement CaseManagement { get; }

        public AssignCaseManagerModel(IClienteleServices userServices, IClienteleStaffServices clienteleStaffServices , ICaseManagement caseManagement,ILogger<AssignCaseManagerModel> logger)
        {
            this.userServices = userServices;
            this.clienteleStaffServices = clienteleStaffServices;
            CaseManagement = caseManagement;
            this.logger = logger;
        }
        public async Task<IActionResult> OnGet(string rct)
        {
            var usersInRole = await clienteleStaffServices.GetUserByRoles("CaseManager");
            var usersInClaim =  await clienteleStaffServices.GetUserByClaims("caseupdate");
            var clientCase = await CaseManagement.GetCaseByReceipt(rct);

            var currentCaseManager = await userServices.GetUserByIds(new List<ObjectId>() { clientCase.CaseManagerId });
            CurrentCaseManager = currentCaseManager.FirstOrDefault();

            CaseManagers = usersInClaim.Where(x => usersInClaim.Any(y => y.Id == x.Id)).ToList();
            Receipt = rct;
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {

            try
            {
                var clientCase = await CaseManagement.GetCaseByReceipt(Receipt);
                var changeCMDetail = await userServices.GetByEmail(SeletedEmail);
                var oldCaseManagerID = clientCase.CaseManagerId;
                clientCase.CaseManagerId = changeCMDetail.Id;

                await CaseManagement.UpdateCaseManager(clientCase);
                logger.LogInformation(LogEvents.ChangeCaseManager, $"Changed from {oldCaseManagerID} to {changeCMDetail.Id}");
                
            }
            catch (Exception error)
            {
                logger.LogCritical(LogEvents.ChangeCaseManagerFail, $"Fail to change CM",error);
            }
            return RedirectToPage($"/Case/CaseDetail",new { rct=Receipt});
        }
    }
}