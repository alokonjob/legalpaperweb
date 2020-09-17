using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Address;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using User;
using Users;

namespace PaperWorks.Areas.Identity.Pages.Account.Manage
{
    public class AddressModel : PageModel
    {
        private readonly SignInManager<Clientele> signInManager;
        private readonly UserManager<Clientele> userManager;
        private readonly StateStaticService fetchIndianStates;
        private readonly CountryService countryService;
        private readonly IClienteleServices userServices;
        public List<SelectListItem> AvailableStates;
        public List<SelectListItem> AvailableCountries;


        [BindProperty(SupportsGet =true)]
        public FundamentalAddress.UserAddress AddressOfUser { get; set; }
        public AddressModel(SignInManager<Clientele> signInManager, UserManager<Clientele> userManager, StateStaticService fetchIndianStates, CountryService fetchCountry, IClienteleServices userServices)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.fetchIndianStates = fetchIndianStates;
            this.countryService = fetchCountry;
            this.userServices = userServices;
            AvailableStates = fetchIndianStates.GetStates();
            AvailableStates.Where(x => x.Value == "Delhi").FirstOrDefault().Selected = true;
            AvailableCountries = fetchCountry.GetCountries();

            AvailableCountries.Where(x => x.Value == "IN").FirstOrDefault().Selected = true;
        }
        public void OnGetAsync()
        {
            var user = userManager.GetUserAsync(User).Result;
            if (user == null)
            {
                RedirectToPage("Error");
            }
            if(user.Addresses != null) AddressOfUser = user.Addresses.FirstOrDefault();

        }

        public async void OnPostSaveAddressAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                RedirectToPage("Error");
            }
            await userServices.SaveAddress(user, AddressOfUser);
        }
    }
}
