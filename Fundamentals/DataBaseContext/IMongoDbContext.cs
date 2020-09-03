using MongoDB.Driver;

namespace Fundamentals.DbContext
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(IMongoClient client, string collectionName);
        IMongoClient GetMongoClient();
    }
}