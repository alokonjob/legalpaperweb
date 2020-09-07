using Fundamentals.DbContext;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseManagementSpace
{
    public class CaseRepository : ICaseRepository
    {
        IMongoClient client = null;
        private readonly IMongoCollection<Case> _caseCollection;
        public CaseRepository(IMongoDbContext mongoContext)
        {
            client = mongoContext.GetMongoClient();
            _caseCollection = mongoContext.GetCollection<Case>(client, "Case");
        }
        public async Task<ObjectId> AddCase(Case clientCase)
        {
            await _caseCollection.InsertOneAsync(clientCase);
            return clientCase.CaseId;
        }

        public async Task<List<Case>> GetAll()
        {
            var filter = Builders<Case>.Filter.Empty;
            var allDocs = await _caseCollection.FindAsync<Case>(filter);
            return allDocs.ToList();
        }
    }
}
