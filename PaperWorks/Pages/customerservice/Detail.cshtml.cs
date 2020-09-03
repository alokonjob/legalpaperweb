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
        public DetailModel(IEnabledServices enableServiceManager)
        {
            this.enableServiceManager = enableServiceManager;
        }

        public void OnGet(string servicename, string city)
        {

            //https://www.mikesdotnetting.com/article/346/using-resource-files-in-razor-pages-localisation
            //https://www.c-sharpcorner.com/blogs/securing-the-url-parameterother-sensitive-data-using-net-core-dataprotectortokenprovider
            CurrentDisplayService = enableServiceManager.GetEnabledService(servicename, city);
            var serviceEnableId = CurrentDisplayService?.EnableId.ToString();
            HttpContext.Session.SetString("DataBaseId", serviceEnableId);
        }
    }
}