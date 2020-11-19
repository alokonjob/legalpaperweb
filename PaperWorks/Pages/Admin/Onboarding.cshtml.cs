using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Address;
using Consultant;
using FundamentalAddress;
using Fundamentals;
using Fundamentals.Managers;
using Fundamentals.Unit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using User;
using Users;

namespace PaperWorks
{
    public class EnabledServiceSelection
    {
        private string _serviceId { get; set; }
        public EnabledServices enabledService { get; set; }
        public string ServiceId { get { return  _serviceId; } set { _serviceId = value; } } 
        public string FeeType { get; set; }
        public bool IsEnabled { get; set; }
    }
    [Authorize(Policy = "AddEditPeople")]
    public class OnboardingModel : PageModel
    {
        private readonly IClienteleServices clientServices;
        private readonly IEnabledServices enabledServices;
        private readonly IConsultantCareerManagement consultantCareerManager;
        private readonly IGeographyManagement geoManager;
        private readonly CountryService countryService;


        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Phone Number")]
            [RegularExpression(@"^([1-9][0-9]{9})$", ErrorMessage = "Invalid Phone Number.")]
            [Display(Name = "Mobile number")]
            public string PhoneNumber { get; set; }
            [Display(Name = "Phone number country")]
            public string PhoneNumberCountryCode { get; set; }
            public UserAddress AddressOfUser { get; set; }

            public ConsultantVerificationDetails ConsultantDocuments { get; set; }
            public ConsultantTaxDetails ConsultantTaxDetails { get; set; }
            //[Required]
            //[Display(Name = "Adhaar")]
            //[RegularExpression(@"^[2-9]{1}[0-9]{3}\\s[0-9]{4}\\s[0-9]{4}$", ErrorMessage = "Please Enter a Valid Adhaar")]
            //public string Adhaar { get; set; }
            //[Required]
            //[Display(Name = "Pancard")]
            //[RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Please Enter a Valid Pan")]
            //public string PanCard { get; set; }
            //public string VoterId { get; set; }
            //public string DrivingLicense { get; set; }
            //public string GST { get; set; }

            public string City { get; set; }

        }

        public OnboardingModel(IClienteleServices clientServices ,IEnabledServices enabledServices,IConsultantCareerManagement consultantCareerManager, IGeographyManagement geoManager, CountryService countryService, StateStaticService fetchIndianStates)
        {
            this.clientServices = clientServices;
            this.enabledServices = enabledServices;
            this.consultantCareerManager = consultantCareerManager;
            this.geoManager = geoManager;
            this.countryService = countryService;
            AvailableCountries = countryService.GetCountries();
            AvailableCountries.Where(x => x.Value == "IN").FirstOrDefault().Selected = true;
            AvailableStates = fetchIndianStates.GetStates();
            AvailableStates.Where(x => x.Value == "Delhi").FirstOrDefault().Selected = true;
            AllGeographies = geoManager.FetchAllGeographies();
            GeoSelection = AllGeographies.Select(x => new SelectListItem(x.City, x.City, false)).ToList();
            GeoSelection.Insert(0, new SelectListItem("None", "None", false));
        }
        [BindProperty(SupportsGet = true)]
        public InputModel Input { get; set; }
        public List<SelectListItem> AvailableCountries { get; }
        public List<SelectListItem> AvailableStates;
        public List<Geography> AllGeographies { get; set; }
        public List<SelectListItem> GeoSelection;
        public List<string> TypeOfFee = new List<string>() { "Fixed", "Negotiable" };
        [BindProperty(SupportsGet = true)]
        public List<EnabledServiceSelection> AllEnabledService { get; set; }
        public void OnGet()
        {

        }



        public PartialViewResult OnGetEnabledService(string city)
        {
            if (string.IsNullOrEmpty(city) || city == "None") city = "delhi";
            city = city.ToLower();
            var AllEnabledService = enabledServices.GetEnabledServicesInCity(city).Select(x => new EnabledServiceSelection() { enabledService = x, IsEnabled = false, FeeType = "Fixed" }).ToList();
            if (AllEnabledService == null) AllEnabledService = new List<EnabledServiceSelection>();
            return Partial("_ServicesListing", AllEnabledService);
        }

        public async Task<IActionResult> OnPostAddConsultant()
        {
            try
            {
                var allTasks = new List<Task>();
                string returnUrl = Url.Content("~/");
                var password = PasswordGenerator.GenerateRandomPassword();
                var Role = "Consultant";
                Claim consultantClaim = new Claim("access", "consultant");
                var result = await clientServices.CreateLogin(Input.Name, Input.Email, Input.PhoneNumberCountryCode, Input.PhoneNumber, Input.AddressOfUser, password, Role, consultantClaim);
                if (result.ResultValue == ResultValue.Success)
                {
                    
                    var user = (Clientele)result.SomeGuy;

                    #region sendAccontConfirmationEmail
                    var code = clientServices.GetEmailConfirmationCode(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    allTasks.Add(clientServices.SendAccountConfirmEmailOnLoginCreation(Input.Email, callbackUrl));
                    #endregion
                    #region SendPasswordEmail
                    var sendPasswordMail = clientServices.SendNewPassword(Input.Email, password);
                    allTasks.Add(sendPasswordMail);
                    #endregion

                    #region CreateConsultantCareer
                    ConsultantCareer consultantCareer = new ConsultantCareer()
                    {
                        ConsultantId = user.Id,
                        Ratings = new List<double>() { 4 },
                        TotalCases = 0,
                        ServicesOffered = AllEnabledService.Where(x => x.IsEnabled == true).Select(x => new ServicesOfConsultant() { EnabledServiceId = x.enabledService.EnableId, Fee = x.enabledService.CostToConsultant, FeeType = x.FeeType, IsEnabled = true }).ToList()
                    };
                    var consultant = consultantCareerManager.IntroduceConsultantCareer(consultantCareer).Result;
                    Input.ConsultantDocuments.ConsultantId = Input.ConsultantTaxDetails.ConsultantId =  consultant.ConsultantId;
                    allTasks.Add( consultantCareerManager.AddConsultantDocuments(Input.ConsultantDocuments));
                    //we are saving PAN redundantly 
                    Input.ConsultantTaxDetails.PanCard = Input.ConsultantDocuments.PanCard;
                    allTasks.Add( consultantCareerManager.AddConsultantTaxDetails(Input.ConsultantTaxDetails));
                    #endregion
                    Task.WaitAll(allTasks.ToArray());
                    return RedirectToPage("/Consultant/ConsultantManagement",new { userEmail  = Input.Email });
                }
                else
                {
                    foreach (var error in result.ResultMessages)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    
                }
                
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
            return Page();
        }
    }
}
