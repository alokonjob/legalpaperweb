using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals.Unit
{
    public class ServiceDetailedInformation
    {
        public string DisplayName { get; set; }
        public ServiceInnerInformation Overview { get; set; }
        public ServiceInnerInformation Process { get; set; }

        public ServiceInnerInformation Documents { get; set; }
        public List<FAQ> Faqs { get; set; }

    }

    public class ServiceInnerInformation
    {
        //We actually dont need this.
        //We are saving the Localized Resource Id this.
        // We can even prepare it in the run time . concatenation of serviceName+"OVerviewText" o serviceName + "overViewTitle"
        public string Title { get; set; }
        public string  Text { get; set; }
        public List<string> Urls{ get; set; }
        public List<string> Attachments { get; set; }
    }

    public class FAQ
    {
        public string Question { get; set; }
        public string Answer{ get; set; }

    }
}
