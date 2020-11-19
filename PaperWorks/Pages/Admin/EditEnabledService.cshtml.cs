using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fundamentals.Managers;
using Fundamentals.Unit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PaperWorks
{
    public class EditEnabledServiceModel : PageModel
    {
        private readonly IEnabledServices enableServiceManager;
        [BindProperty(SupportsGet = true)]
        public EnabledServices EnabledService { get; set; }

        public EditEnabledServiceModel(IEnabledServices enableServiceManager)
        {
            this.enableServiceManager = enableServiceManager;
        }
        public void OnGet(string es)
        {
            EnabledService = enableServiceManager.GetEnabledServiceById(es);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            enableServiceManager.UpdateEnabledService(EnabledService);
            return RedirectToPage("/Admin/EnabledServices", new { city= EnabledService.Location.City });
        }
    }
}