﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Address
{
    public class CountryService
    {
        private readonly IHostingEnvironment _environment;
        private readonly Lazy<List<SelectListItem>> _countries;

        public CountryService(IHostingEnvironment environment)
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
