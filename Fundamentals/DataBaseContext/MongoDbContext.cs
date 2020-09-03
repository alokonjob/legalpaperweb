using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals.DbContext
{
    public class MongoDbContext : IMongoDbContext
    {

        public IMongoClient GetMongoClient()
        {
            IMongoClient client = new MongoClient("mongodb+srv://alok:Host123456@mflix.cxpea.azure.mongodb.net/onjob2?authSource=admin&replicaSet=atlas-x3ev7x-shard-0&readPreference=primary&appname=MongoDB%20Compass&ssl=true");
            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);
            return client;
        }

        public IMongoCollection<T> GetCollection<T>(IMongoClient client, string collectionName)
        {

            return client.GetDatabase("onjob2").GetCollection<T>(collectionName);
        }
    }
}
