using CaseManagementSpace;
using Fundamentals.DbContext;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CaseManagement
{
    public class CaseUpdateRepository : ICaseUpdateRepository
    {
        IMongoClient client = null;
        private readonly IMongoCollection<CaseUpdate> _caseUpdateCollection;
        public CaseUpdateRepository(IMongoDbContext mongoContext)
        {
            client = mongoContext.GetMongoClient();
            _caseUpdateCollection = mongoContext.GetCollection<CaseUpdate>(client, "CaseUpdate");
        }

        public async Task<CaseUpdate> PostUpdate(CaseUpdate newUpdate)
        {
            await _caseUpdateCollection.InsertOneAsync(newUpdate);
            return newUpdate;
        }

        public async Task<List<CaseUpdate>> GetAllUpdates(string caseId)
        {
            var filter = Builders<CaseUpdate>.Filter.Eq(x => x.CaseId, ObjectId.Parse(caseId));
            var updates = await _caseUpdateCollection.FindAsync<CaseUpdate>(filter);
            return await updates.ToListAsync();
        }
    }
}
