using Asgard;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals.DbContext
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IHeimdall heimdall;

        public MongoDbContext(IHeimdall heimdall)
        {
            this.heimdall = heimdall;
        }
        public IMongoClient GetMongoClient()
        {
            var mongoConnection = heimdall.GetSecretValue("MongoConnection");
            IMongoClient client = new MongoClient(mongoConnection);
            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);
            return client;
        }

        public IMongoCollection<T> GetCollection<T>(IMongoClient client, string collectionName)
        {

            return client.GetDatabase(heimdall.GetSecretValue("MongoDbName")).GetCollection<T>(collectionName);
        }
    }
}
