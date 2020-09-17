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

        public async Task<Case> GetCaseById(string caseId)
        {
            ObjectId CaseObjectId = ObjectId.Parse(caseId);
            var filter = Builders<Case>.Filter.Eq(x => x.CaseId, CaseObjectId);
            var specificCase = await _caseCollection.FindAsync<Case>(filter);
            return specificCase.FirstOrDefault();

        }

        public async Task<Case> UpdateConsultant(Case caseToUpdate)
        {
            var filter = Builders<Case>.Filter.Eq(t => t.CaseId, caseToUpdate.CaseId);
            var updatedCaseWithConsultantDetails = await _caseCollection.FindOneAndUpdateAsync<Case>(
                    filter,
                    Builders<Case>.Update
                    .Set(
                        t => t.Order.ConsultantEmail,
                        caseToUpdate.Order.ConsultantEmail)
                    .Set(
                        t => t.Order.ConsultantPhone,
                        caseToUpdate.Order.ConsultantPhone)
                    .Set(
                        t => t.CaseId,
                        caseToUpdate.CaseId)
                    );
            return updatedCaseWithConsultantDetails;
        }
    }
}
