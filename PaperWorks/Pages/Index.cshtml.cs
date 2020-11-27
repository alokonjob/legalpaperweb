using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fundamentals.Managers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using User;

namespace PaperWorks.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IGeographyManagement geoManager;
        private readonly ILogger<IndexModel> _logger;
        List<string> Cities = new List<string>();

        public IndexModel(IGeographyManagement geoManager, ILogger<IndexModel> logger)
        {
            this.geoManager = geoManager;
            _logger = logger;
        }

        public IActionResult OnGet(string city = "")
        {
            
            if (string.IsNullOrEmpty(city) == false)
            {
                Set("location", city.ToLower(), 100);
            }

            
            if (User.IsFinanceUser())
            {
                return RedirectToPage("/Case/CaseListing");
            }
            if (User.IsCaseManager())
            {
                return RedirectToPage("/Case/CaseListing");
            }
            if (User.IsConsultant())
            {
                return RedirectToPage("/Consultant/MyCases");
            }
            if (User.IsWebAdmin())
            {
                return RedirectToPage("/Consultant/ConsultantManagement");
            }
            if (User.IsFounder())
            {
                return RedirectToPage("/Case/CaseListing");
            }
            if (!string.IsNullOrEmpty(city))
            {
               return RedirectToPage("/Index");
            }
            return Page();
        }


        public string Get(string key)
        {
            return Request.Cookies[key];
        }
        /// <summary>  
        /// set the cookie  
        /// </summary>  
        /// <param name="key">key (unique indentifier)</param>  
        /// <param name="value">value to store in cookie object</param>  
        /// <param name="expireTime">expiration time</param>  
        public void Set(string key, string value, int? expireTime)
        {
            CookieOptions option = new CookieOptions();
            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddDays(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddDays(10);
            Response.Cookies.Append(key, value, option);
        }
    }
}
