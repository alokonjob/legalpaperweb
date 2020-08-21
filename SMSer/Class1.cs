using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Twilio;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SMSer
{
    public static class InitTwilio
    {
        public static void Init(IConfiguration Configuration)
        {
            var accountSid = Configuration["Keys:Twilio:AccountSID"];
            var authToken = Configuration["Keys:Twilio:AuthToken"];
            TwilioClient.Init(accountSid, authToken);
        }
    }

    public class TwilioVerifySettings
    {
        public string VerificationServiceSID { get; set; }
    }

    public class PhoneCountryService
    {
        private readonly IHostingEnvironment _environment;
        private readonly Lazy<List<SelectListItem>> _countries;

        public PhoneCountryService(IHostingEnvironment environment)
        {
            _environment = environment;
            _countries = new Lazy<List<SelectListItem>>(LoadCountries);
        }

        public List<SelectListItem> GetCountries()
        {
            return _countries.Value;
        }

        private List<SelectListItem> LoadCountries()
        {
            var fileInfo = _environment.ContentRootFileProvider.GetFileInfo("wwwroot\\static\\country.json");

            using (var stream = fileInfo.CreateReadStream())
            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<List<SelectListItem>>(jsonTextReader);
            }
        }
    }
}
