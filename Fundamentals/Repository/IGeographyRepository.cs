using Fundamentals.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals.Repository
{
    public interface IGeographyRepository
    {
        void Add(Geography geo);
        List<Geography> GetAll();
        Geography GetByCity(string name);
    }
}
