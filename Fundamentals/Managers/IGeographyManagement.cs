using Fundamentals.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals.Managers
{
    public interface IGeographyManagement
    {
        void AddNewGeography(Geography geography);
        List<Geography> FetchAllGeographies();
        Geography FetchByCity(string name);
    }
}
