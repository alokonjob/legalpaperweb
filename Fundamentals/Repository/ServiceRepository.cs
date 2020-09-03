using Fundamentals.DbContext;
using Fundamentals.Unit;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals.Repository
{
    public class ServiceRepository : IServiceRepository
    {
        IMongoClient client = null;
        private readonly IMongoCollection<Service> _servicesCollection;
        private readonly IMongoDbContext mongoContext;

        public ServiceRepository(IMongoDbContext mongoContext)
        {
            this.mongoContext = mongoContext;
            client = mongoContext.GetMongoClient();
            _servicesCollection = mongoContext.GetCollection<Service>(client, "ServicesMaster");
        }
        public void Add(Service service)
        {
            if (service.DetailedDisplayInfo == null)
            {
                service.DetailedDisplayInfo = new ServiceDetailedInformation();
                service.DetailedDisplayInfo.DisplayName = "Marriage Certificate";
                service.DetailedDisplayInfo.Overview = new ServiceInnerInformation();
                service.DetailedDisplayInfo.Overview.Title = "marriagecertificateoverviewTitle";
                service.DetailedDisplayInfo.Overview.Text = "marriagecertificateoverviewText";
                service.DetailedDisplayInfo.Process = new ServiceInnerInformation();
                service.DetailedDisplayInfo.Process.Title = "marriagecertificateprocessTitle";
                service.DetailedDisplayInfo.Process.Text = "marriagecertificateprocessText";
                service.DetailedDisplayInfo.Documents = new ServiceInnerInformation();
                service.DetailedDisplayInfo.Documents.Title = "marriagecertificatedocumentsTitle";
                service.DetailedDisplayInfo.Documents.Text = "marriagecertificatedocumentsText";
            }
            _servicesCollection.InsertOne(service);
        }

        public List<Service> GetAll()
        {
            var filter = Builders<Service>.Filter.Empty;
            return _servicesCollection.Find<Service>(filter).ToList<Service>();
        }

        public Service GetByName(string name)
        {
            return _servicesCollection.Find<Service>(x => x.Name == name).FirstOrDefault();
        }
    }
}
