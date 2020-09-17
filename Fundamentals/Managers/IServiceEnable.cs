using Fundamentals.DbContext;
using Fundamentals.Unit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals.Managers
{
    public interface IEnabledServices
    {
        Result EnableServiceInLocation(EnabledServices service);
        EnabledServices GetEnabledService(string serviceName, string city);
        EnabledServices GetEnabledServiceById(string Id);
        List<EnabledServices> GetEnabledServicesInCity(string city);
    }

    public class EnabledServicesManager : IEnabledServices
    {
        private readonly IServiceEnableRepository serviceEnableRepo;
        private readonly IServiceManagement serviceManagement;
        private readonly IGeographyManagement geographyManagement;

        public EnabledServicesManager(IServiceEnableRepository serviceEnableRepo, IServiceManagement serviceManagement, IGeographyManagement geographyManagement)
        {
            this.serviceEnableRepo = serviceEnableRepo;
            this.serviceManagement = serviceManagement;
            this.geographyManagement = geographyManagement;
        }
        public Result EnableServiceInLocation(EnabledServices service)
        {
            try
            {
                var fullService = serviceManagement.FetchServiceByName(service.ServiceDetail.Name);
                var geography = geographyManagement.FetchByCity(service.Location.City);
                if (fullService.IsActive == false || geography.IsActive == false)
                {
                    return new Result(ResultValue.ErrorAndFatal, ErrorCode.ServiceOrGeographyInactive, "Service or Location can not be inactive");
                }
                service.ServiceDetail = fullService;
                service.Location = geography;
                service.CostToCustomer = 17000;
                service.CostToConsultant = 10000;
                service.Steps = new List<ServiceStep>()
                {
                    new ServiceStep(){ Name="Customer Dcouments",Description="Get required documents from Customer",Status=""},
                    new ServiceStep(){ Name="Application in Govt Office",Description="Apply in govt office",Status=""},
                    new ServiceStep(){ Name="Go to Registrar Office",Description="Signing ceremony",Status=""},
                    new ServiceStep(){ Name="Reecive Marriage Certificate",Description="Receive from Office",Status=""},
                    new ServiceStep(){ Name="Provide to Customer",Description="Provide to Customer",Status=""},
                };
                serviceEnableRepo.Add(service);

            }
            catch (Exception error)
            {
                    return new Result(ResultValue.ErrorAndFatal, ErrorCode.RepositoryError, "Error While Enabling Service");
            }
            return new Result(ResultValue.Success, ErrorCode.None, "Successfully Saved");
        }

        public EnabledServices GetEnabledService(string serviceName, string city)
        {
            var enabledService = serviceEnableRepo.GetEnabledService(serviceName, city);
            return enabledService;
        }

        public EnabledServices GetEnabledServiceById(string Id)
        {
            var enabledService = serviceEnableRepo.GetEnabledServiceById(Id);
            return enabledService;
        }

        public List<EnabledServices> GetEnabledServicesInCity(string city)
        {
            return serviceEnableRepo.GetEnabledServicesInCity(city);
        }
    }

    public interface IServiceEnableRepository
    {
        void Add(EnabledServices service);
        EnabledServices GetEnabledService(string serviceName, string city);
        EnabledServices GetEnabledServiceById(string Id);
        List<EnabledServices> GetEnabledServicesInCity(string city);
    }

    public class ServiceEnableRepository : IServiceEnableRepository
    {
        IMongoClient client = null;
        private readonly IMongoCollection<EnabledServices> _enabledServicesCollection;
        private readonly IMongoDbContext mongoContext;

        public ServiceEnableRepository(IMongoDbContext mongoContext)
        {
            this.mongoContext = mongoContext;
            client = mongoContext.GetMongoClient();
            _enabledServicesCollection = mongoContext.GetCollection<EnabledServices>(client,"EnabledServices");
        }

        public void Add(EnabledServices service)
        {
            _enabledServicesCollection.InsertOne(service);
        }

        public EnabledServices GetEnabledService(string serviceName, string city)
        {
            return _enabledServicesCollection.Find<EnabledServices>(x =>
            x.ServiceDetail.Name == serviceName
            &&
            x.Location.City == city).FirstOrDefault();
        }

        public EnabledServices GetEnabledServiceById(string Id)
        {
            return _enabledServicesCollection.Find<EnabledServices>(x =>x.EnableId == Id).FirstOrDefault();
        }

        public List<EnabledServices> GetEnabledServicesInCity(string city)
        {
            var filter = Builders<EnabledServices>.Filter.Eq(t => t.Location.City, city);
            var allServicesInCity = _enabledServicesCollection.Find(filter).ToList();
            return allServicesInCity;
        }
    }
}
