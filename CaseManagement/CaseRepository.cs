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

        public async Task<List<Case>> GetAllCasesOfUser(string userEmail)
        {
            var filter = Builders<Case>.Filter.Eq(t=>t.Order.CustomerEmail,userEmail);
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

        public async Task<Case> GetCaseByReceipt(string receipt)
        {
            var filter = Builders<Case>.Filter.Eq(x => x.Order.Receipt, receipt);
            var specificCase = await _caseCollection.FindAsync<Case>(filter);
            return specificCase.FirstOrDefault();
        }

        public async Task<Case> UpdateConsultant(Case caseToUpdate)
        {
            var filter = Builders<Case>.Filter.Eq(t => t.CaseId, caseToUpdate.CaseId);
            var updatedCaseWithConsultantDetails = caseToUpdate.PreviousConsultantId == null || caseToUpdate.PreviousConsultantId.Count == 0 ? await _caseCollection.FindOneAndUpdateAsync<Case>(
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
                    .Set(
                        t => t.CurrentConsultantId,
                        caseToUpdate.CurrentConsultantId)
                    .Set(
                        t => t.PreviousConsultantId,
                        caseToUpdate.PreviousConsultantId)
                    ) :

                    await _caseCollection.FindOneAndUpdateAsync<Case>(
                    filter,
                    Builders<Case>.Update
                    .Push<ObjectId>(e=>e.PreviousConsultantId, caseToUpdate.PreviousConsultantId[0])
                    .Set(
                        t => t.Order.ConsultantEmail,
                        caseToUpdate.Order.ConsultantEmail)
                    .Set(
                        t => t.Order.ConsultantPhone,
                        caseToUpdate.Order.ConsultantPhone)
                    .Set(
                        t => t.CaseId,
                        caseToUpdate.CaseId)
                    .Set(
                        t => t.CurrentConsultantId,
                        caseToUpdate.CurrentConsultantId)
                    )
                    ;
            return updatedCaseWithConsultantDetails;
        }

        public async Task<List<Case>> GetCasesOfConsultant(string consultantId)
        {
            ObjectId ConsultantObjectId = ObjectId.Parse(consultantId);
            var filter = Builders<Case>.Filter.Eq(x => x.CurrentConsultantId, ConsultantObjectId);
            var specificCase = await _caseCollection.FindAsync<Case>(filter);
            return specificCase.ToList();
        }

        public async Task<List<Case>> GetCasesOfCaseManager(string caseManagerId)
        {
            ObjectId CaseManagerObjectId = ObjectId.Parse(caseManagerId);
            var filter = Builders<Case>.Filter.Eq(x => x.CaseManagerId, CaseManagerObjectId);
            var specificCase = await _caseCollection.FindAsync<Case>(filter);
            return specificCase.ToList();
        }
    }
}
