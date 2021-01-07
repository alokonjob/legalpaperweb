using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fundamentals.Managers;
using Fundamentals.Unit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PaperWorks
{
    public class EditEnabledServiceModel : PageModel
    {
        private readonly IEnabledServices enableServiceManager;
        [BindProperty(SupportsGet = true)]
        public EnabledServices EnabledService { get; set; }

        public List<SelectListItem> ServiceTypes;

        public EditEnabledServiceModel(IEnabledServices enableServiceManager)
        {
            this.enableServiceManager = enableServiceManager;
        }
        public void OnGet(string es)
        {
            EnabledService = enableServiceManager.GetEnabledServiceById(es);
            ServiceTypes = new List<SelectListItem>() { new SelectListItem(EnableServiceType.Individual.ToString(), EnableServiceType.Individual.ToString()), new SelectListItem(EnableServiceType.Corporate.ToString(), EnableServiceType.Corporate.ToString()) };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            enableServiceManager.UpdateEnabledService(EnabledService);
            return RedirectToPage("/Admin/EnabledServices", new { city= EnabledService.Location.City });
        }
    }
}