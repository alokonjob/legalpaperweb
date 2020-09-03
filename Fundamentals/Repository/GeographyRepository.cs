using Fundamentals.DbContext;
using Fundamentals.Unit;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals.Repository
{
    public class GeographyRepository : IGeographyRepository
    {
        IMongoClient client = null;
        private readonly IMongoCollection<Geography> _geoCollection;
        private readonly IMongoDbContext mongoContext;

        public GeographyRepository(IMongoDbContext mongoContext)
        {
            this.mongoContext = mongoContext;
            client = mongoContext.GetMongoClient();
            _geoCollection = mongoContext.GetCollection<Geography>(client, "GeographyMaster");
        }
        public void Add(Geography geo)
        {
            _geoCollection.InsertOne(geo);
        }

        public List<Geography> GetAll()
        {
            var filter = Builders<Geography>.Filter.Empty;
            return _geoCollection.Find<Geography>(filter).ToList();
        }

        public Geography GetByCity(string city)
        {
            return _geoCollection.Find<Geography>(x => x.City == city).FirstOrDefault();
        }
    }
}
