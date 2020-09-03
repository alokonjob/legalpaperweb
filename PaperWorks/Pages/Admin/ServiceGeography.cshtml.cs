using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Address;
using Fundamentals.Unit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Fundamentals;
using Fundamentals.Managers;

namespace PaperWorks
{
    public class ServiceGeographyModel : PageModel
    {
        private readonly IServiceManagement serviceManagement;
        private readonly IGeographyManagement geoManager;
        private readonly IEnabledServices enableService;
        private readonly StateStaticService fetchIndianStates;
        public List<SelectListItem> AvailableStates;
        public List<SelectListItem> ServiceSelection;
        public List<SelectListItem> GeoSelection;
        public IEnumerable<SelectListItem> ServiceTypes { get; set; }
        public ServiceGeographyModel(IServiceManagement serviceManagement, IGeographyManagement geoManager, IEnabledServices enableService, IHtmlHelper htmlHelper, StateStaticService fetchIndianStates)
        {
            this.serviceManagement = serviceManagement;
            this.geoManager = geoManager;
            this.enableService = enableService;
            this.fetchIndianStates = fetchIndianStates;
            AvailableStates = fetchIndianStates.GetStates();
            AvailableStates.Where(x => x.Value == "Delhi").FirstOrDefault().Selected = true;
            AllServices = serviceManagement.FetchAllServices();
            AllGeographies = geoManager.FetchAllGeographies();
            ServiceSelection = AllServices.Select(x => new SelectListItem(x.Name, x.Name)).ToList();
            GeoSelection = AllGeographies.Select(x => new SelectListItem(x.City, x.City)).ToList();
            ServiceTypes = htmlHelper.GetEnumSelectList<ServiceType>();
        }
        [BindProperty]
        public Service InputService { get; set; }
        [BindProperty]
        public Geography InputGeography { get; set; }
        [BindProperty]
        public EnabledServices InputEnableService { get; set; }
        public List<Service> AllServices { get; set; }
        public List<Geography> AllGeographies { get; set; }
        
        public void OnGet()
        {
            
        }

        public void OnPostAddMasterService()
        {
            serviceManagement.AddNewService(InputService);

        }
        public void OnPostAddMasterGeographyService()
        {
            geoManager.AddNewGeography(InputGeography);

        }

        public void OnPostEnableService()
        {
            enableService.EnableServiceInLocation(InputEnableService);

        }
    }
}