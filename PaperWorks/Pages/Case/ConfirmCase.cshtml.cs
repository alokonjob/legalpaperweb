using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CaseManagementSpace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using User;

namespace PaperWorks
{
    public class ConfirmCasemModel : PageModel
    {
        private readonly ICaseManagement caseManagementService;
        private readonly IClienteleServices userServices;

        public string MessageForConsultant { get; set; }
        public ConfirmCasemModel(ICaseManagement caseManagementService, IClienteleServices userServices)
        {
            this.caseManagementService = caseManagementService;
            this.userServices = userServices;
        }
        public async Task<IActionResult> OnGet(string userId, string code, string rct)
        {
            if (userServices.IsSignedIn(User) == false)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = $"/Case/ConfirmCase?userId={userId}&code={code}&rct={rct}" });
            }
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            rct = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(rct));
            var consultantCase = await caseManagementService.GetCaseByReceipt(rct);
            if (string.Compare(consultantCase.CaseConfirmationCode, code, false) == 0)
            {
                await caseManagementService.AcceptCase(consultantCase, userId);
                await caseManagementService.ChangeStatus(rct, CaseStatus.InProgress);
                MessageForConsultant = "Thank you For Accepting the Case";
            }
            else
            {
                MessageForConsultant = "Case can not be assigned at this moment, please check with Case Manager";
            }
            return Page();
        }
    }
}