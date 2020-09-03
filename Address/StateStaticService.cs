using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Address
{
    public class StateStaticService
    {
        private readonly IHostingEnvironment _environment;
        private readonly Lazy<List<SelectListItem>> _states;

        public StateStaticService(IHostingEnvironment environment)
        {
            _environment = environment;
            _states = new Lazy<List<SelectListItem>>(LoadStates);
        }

        public List<SelectListItem> GetStates()
        {
            return _states.Value;
        }

        private List<SelectListItem> LoadStates()
        {
            var fileInfo = _environment.ContentRootFileProvider.GetFileInfo("wwwroot\\static\\indianstatesuts.json");

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
