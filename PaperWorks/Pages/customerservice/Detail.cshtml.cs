using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fundamentals.Managers;
using Fundamentals.Unit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PaperWorks
{
    public class DetailModel : PageModel
    {
        private readonly IEnabledServices enableServiceManager;
        public EnabledServices CurrentDisplayService = null;
        public string ServiceBookingText { get; set; }
        Random r = new Random();
        public DetailModel(IEnabledServices enableServiceManager)
        {
            this.enableServiceManager = enableServiceManager;
        }

        public IActionResult OnGet(string servicename)
        {

            //https://www.mikesdotnetting.com/article/346/using-resource-files-in-razor-pages-localisation
            //https://www.c-sharpcorner.com/blogs/securing-the-url-parameterother-sensitive-data-using-net-core-dataprotectortokenprovider
            string city = string.IsNullOrEmpty(Request.Cookies["location"]) ? "delhi" : Request.Cookies["location"].ToLower();

            CurrentDisplayService = enableServiceManager.GetEnabledService(servicename, city);
            if (CurrentDisplayService == null || CurrentDisplayService.IsActive == false)
            {
                return RedirectToPage("/ComingToFetchSoon");
            }

            var serviceEnableId = CurrentDisplayService?.EnableId.ToString();
            ServiceBookingText = CurrentDisplayService.KindofService == EnableServiceType.Corporate ? "Request Callback" : "Book Now";
            HttpContext.Session.SetString("DataBaseId", serviceEnableId);
            
            int genRand = r.Next();
            if (genRand % 2 == 0)
            {
                genRand += 1;
            }
            HttpContext.Session.SetInt32("bkc", genRand);
            return Page();
        }

        public IActionResult OnPost()
        {
            int genRand = r.Next();
            if (genRand % 2 != 0)
            {
                genRand += 1;
            }
            HttpContext.Session.Remove("bkc");
            HttpContext.Session.SetInt32("bkc", genRand);
            return RedirectToPage("/Order/PreCheckout");
        }
    }
}