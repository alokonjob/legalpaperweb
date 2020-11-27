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

        public async Task<List<CaseUpdate>> GetAllUpdates(string caseId,bool includeDeleted)
        {
            var sort = Builders<CaseUpdate>.Sort.Descending("updatedDate");
            var filter = includeDeleted == true ? Builders<CaseUpdate>.Filter.Where(x => x.CaseId == ObjectId.Parse(caseId)) :
                Builders<CaseUpdate>.Filter.Where(x => x.CaseId == ObjectId.Parse(caseId) && x.IsDeleted == false);
            var updates = _caseUpdateCollection.Find<CaseUpdate>(filter).Sort(sort);
            return await updates.ToListAsync();
        }

        public async Task<List<CaseUpdate>> GetMyUpdates(string caseId, string Email)
        {
            var filter = Builders<CaseUpdate>.Filter.Eq(x => x.CaseId, ObjectId.Parse(caseId));
            var updates = await _caseUpdateCollection.FindAsync<CaseUpdate>(x => x.CaseId == ObjectId.Parse(caseId) && (x.UpdatedBy.Email == Email || x.ShareWithConsultantEmail == Email) && x.IsDeleted == false);
            return await updates.ToListAsync();
        }

        public async Task<CaseUpdate> RemoveUpdate(string updateId)
        {
            var filter = Builders<CaseUpdate>.Filter.Eq(x => x.CaseUpdateId, ObjectId.Parse(updateId));
            return await _caseUpdateCollection.FindOneAndUpdateAsync<CaseUpdate>(filter,
                Builders<CaseUpdate>.Update.Set(x => x.IsDeleted, true));
        }
    }
}
