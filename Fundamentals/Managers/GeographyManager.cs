using Fundamentals.Repository;
using Fundamentals.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals.Managers
{
    public class GeographyManagement : IGeographyManagement
    {
        private readonly IGeographyRepository geographyRepository;

        public GeographyManagement(IGeographyRepository geographyRepository)
        {
            this.geographyRepository = geographyRepository;
        }
        public void AddNewGeography(Geography geography)
        {
            geographyRepository.Add(geography);
        }

        public List<Geography> FetchAllGeographies()
        {
            return geographyRepository.GetAll();
        }

        public Geography FetchByCity(string city)
        {
            return geographyRepository.GetByCity(city);
        }

    }
}
