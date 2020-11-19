using Fundamentals.Managers;
using Fundamentals.Unit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaperWorks
{
    public class FullUIGeo
    {
        public string SavedGeo { get; set; }
        public List<Geography> allGeographies = new List<Geography>();
    }
    public class LocationViewComponent : ViewComponent
    {
        private readonly IGeographyManagement geoGraphies;


        

        public LocationViewComponent(IGeographyManagement geoGraphies)
        {
            this.geoGraphies = geoGraphies;
        }

        public IViewComponentResult Invoke()
        {
            FullUIGeo geo = new FullUIGeo();
            geo.allGeographies = geoGraphies.FetchAllGeographies();
            geo.SavedGeo = Get("location").ToUpper();
            return View("Location", geo);
        }

        public string Get(string key)
        {
            return Request.Cookies[key];
        }
    }
}
