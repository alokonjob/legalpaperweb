using Fundamentals.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals.Repository
{
    public interface IServiceRepository
    {
        void Add(Service service);
        List<Service> GetAll();

        Service GetByName(string name);
    }
}
