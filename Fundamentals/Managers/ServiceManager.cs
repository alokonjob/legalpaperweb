using Fundamentals.Repository;
using Fundamentals.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals.Managers
{
    public class ServiceManager : IServiceManagement
    {
        private readonly IServiceRepository serviceRepository;

        public ServiceManager(IServiceRepository serviceRepository)
        {
            this.serviceRepository = serviceRepository;
        }
        public void AddNewService(Service service)
        {
            if (service.DetailedDisplayInfo == null)
            {
                service.DetailedDisplayInfo = new ServiceDetailedInformation();
                service.DetailedDisplayInfo.DisplayName = $"{service.Name}ServiceName";
                service.DetailedDisplayInfo.Overview = new ServiceInnerInformation();
                service.DetailedDisplayInfo.Overview.Title = $"{service.Name}overviewTitle"; // "marriagecertificateoverviewTitle";
                service.DetailedDisplayInfo.Overview.Text = $"{service.Name}overviewText";// "marriagecertificateoverviewText";
                service.DetailedDisplayInfo.Process = new ServiceInnerInformation();
                service.DetailedDisplayInfo.Process.Title = $"{service.Name}processTitle";
                service.DetailedDisplayInfo.Process.Text = $"{service.Name}processText";
                service.DetailedDisplayInfo.Documents = new ServiceInnerInformation();
                service.DetailedDisplayInfo.Documents.Title = $"{service.Name}documentsTitle";
                service.DetailedDisplayInfo.Documents.Text = $"{service.Name}documentsText";
                service.DetailedDisplayInfo.Faqs = new List<FAQ>();
                for (int i = 0; i < 3; i++)
                {
                    service.DetailedDisplayInfo.Faqs.Add(new FAQ() { Question = $"{service.Name}Question{i}", Answer = $"{service.Name}Answer{i}" });
                }
            }

            serviceRepository.Add(service);
        }

        public List<Service> FetchAllServices()
        {
            return serviceRepository.GetAll();
        }

        public Service FetchServiceByName(string name)
        {
            return serviceRepository.GetByName(name);
        }
    }
}
