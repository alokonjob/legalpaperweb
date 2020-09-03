using Fundamentals.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals
{
    public interface IServiceManagement
    {
        void AddNewService(Service service);
        Service FetchServiceByName(string name);
        List<Service> FetchAllServices();
    }
}
