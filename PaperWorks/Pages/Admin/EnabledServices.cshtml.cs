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
    public class EnabledServicesModel : PageModel
    {
        private readonly IEnabledServices enableService;
        public List<EnabledServices> AllEnabledServices { get; set; }
        public EnabledServicesModel(IEnabledServices enableService)
        {
            this.enableService = enableService;
        }
        public async Task<IActionResult> OnGet(string city)
        {
            AllEnabledServices = enableService.GetEnabledServicesInCity(city) ?? new List<EnabledServices>();
            return Page();
        }
    }
}