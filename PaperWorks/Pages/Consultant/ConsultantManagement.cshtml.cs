using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using User;
using Users;

namespace PaperWorks
{
    [Authorize(Policy = "AccessConsultants")]
    public class ConsultantManagementModel : PageModel
    {
        private readonly IClienteleStaffServices clienteleStaffServices;

        public IList<Clientele> Consultants { get; set; }
        public ConsultantManagementModel(IClienteleStaffServices clienteleStaffServices)
        {
            this.clienteleStaffServices = clienteleStaffServices;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            Consultants = await clienteleStaffServices.GetUserByRoles("Consultant");
            return Page();

        }
    }
}