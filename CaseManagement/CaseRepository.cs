using Fundamentals.DbContext;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using User;

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

        public async Task<List<Case>> GetAll(Filters filters = null)
        {


            FilterDefinition<Case> CombinedFilter = GetFilterToApply(filters);
            if (null == CombinedFilter)
            {
                CombinedFilter = Builders<Case>.Filter.Lte(x => x.CurrentStatus, CaseStatus.CaseManagerClosed);
            }
            var allDocs = await _caseCollection.FindAsync<Case>(CombinedFilter);

            return allDocs.ToList();
        }

        public FilterDefinition<Case> GetFilterToApply(Filters filters)
        {
            if (filters == null) return null;
            FilterDefinition<Case> StatusFilter = null;
            FilterDefinition<Case> ReceiptFilter = null;
            FilterDefinition<Case> ServiceTypeFilter = null;
            FilterDefinition<Case> StartDateFilter = null;
            FilterDefinition<Case> EndDateFilter = null;
            FilterDefinition<Case> CombinedFilter = null;

            if (filters != null)
            {
                if (filters.CaseStatus != CaseStatus.None)
                {
                    StatusFilter = Builders<Case>.Filter.Eq(x => x.CurrentStatus, filters.CaseStatus);
                    CombinedFilter = CombinedFilter == null ? StatusFilter : Builders<Case>.Filter.And(CombinedFilter, StatusFilter);

                }


                if (!string.IsNullOrEmpty(filters.Receipt))
                {
                    ReceiptFilter = Builders<Case>.Filter.Eq(x => x.Order.Receipt, filters.Receipt);
                    CombinedFilter = CombinedFilter == null ? ReceiptFilter : Builders<Case>.Filter.And(CombinedFilter, ReceiptFilter);

                }
                if (filters.ServiceType != Fundamentals.Unit.EnableServiceType.None)
                {
                    ServiceTypeFilter = Builders<Case>.Filter.Eq(x => x.Order.CallbackType, filters.ServiceType);
                    CombinedFilter = CombinedFilter == null ? ServiceTypeFilter : Builders<Case>.Filter.And(CombinedFilter, ServiceTypeFilter);

                }

                if (!filters.FromDate.IsDefaultDate() && !filters.ToDate.IsDefaultDate()
                    && filters.FromDate <= filters.ToDate)
                {
                    StartDateFilter = Builders<Case>.Filter.Gte(x => x.CreatedDate, filters.FromDate.SetDayStartDate());
                    EndDateFilter = Builders<Case>.Filter.Lte(x => x.CreatedDate, filters.ToDate.SetDayEndDate());
                    CombinedFilter = CombinedFilter == null ? StartDateFilter & EndDateFilter : Builders<Case>.Filter.And(CombinedFilter, StartDateFilter, EndDateFilter);
                }
            }

            return CombinedFilter;
        }

        public async Task<List<Case>> GetAllCasesOfUser(string userEmail)
        {
            var filter = Builders<Case>.Filter.Eq(t => t.Order.CustomerEmail, userEmail);
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
        public async Task<Case> AcceptCase(Case caseToUpdate, string Email)
        {
            var filter = Builders<Case>.Filter.Eq(t => t.CaseId, caseToUpdate.CaseId);
            var updatedCaseWithConsultantDetails = await _caseCollection.FindOneAndUpdateAsync<Case>(
                    filter,
                    Builders<Case>.Update
                    .Set(
                        t => t.CaseConfirmedBy,
                        Email));
            return updatedCaseWithConsultantDetails;
        }

        public async Task<Case> ChangeStatus(string receipt, CaseStatus caseStatus)
        {
            var filter = Builders<Case>.Filter.Eq(t => t.Order.Receipt, receipt);
            var updatedCaseWithConsultantDetails = await _caseCollection.FindOneAndUpdateAsync<Case>(
                    filter,
                    Builders<Case>.Update
                    .Set(
                        t => t.CurrentStatus,
                        caseStatus));
            return updatedCaseWithConsultantDetails;
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
                    .Set(
                        t => t.CaseConfirmationCode,
                        caseToUpdate.CaseConfirmationCode)
                    .Set(t => t.CurrentStatus,
                    caseToUpdate.CurrentStatus)
                    ) :

                    await _caseCollection.FindOneAndUpdateAsync<Case>(
                    filter,
                    Builders<Case>.Update
                    .Push<ObjectId>(e => e.PreviousConsultantId, caseToUpdate.PreviousConsultantId[0])
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
                        t => t.CaseConfirmationCode,
                        caseToUpdate.CaseConfirmationCode)
                    .Set(t => t.CurrentStatus,
                    caseToUpdate.CurrentStatus)
                    )
                    ;
            return updatedCaseWithConsultantDetails;
        }

        public async Task<Case> UpdateCaseManager(Case caseToUpdate)
        {
            var filter = Builders<Case>.Filter.Eq(t => t.CaseId, caseToUpdate.CaseId);
            var updatedCaseWithConsultantDetails = await _caseCollection.FindOneAndUpdateAsync<Case>(
                    filter,
                    Builders<Case>.Update
                    .Set(
                        t => t.CaseManagerId,
                        caseToUpdate.CaseManagerId)
                    );
            return updatedCaseWithConsultantDetails;
        }

        public async Task<List<Case>> GetCasesOfConsultant(string userEmail)
        {

            var filter = Builders<Case>.Filter.Eq(x => x.Order.ConsultantEmail, userEmail);
            var caseConfirmationFilter = Builders<Case>.Filter.Eq(x => x.CaseConfirmedBy, userEmail);
            var combinedFilter = Builders<Case>.Filter.And(filter, caseConfirmationFilter);
            var specificCase = await _caseCollection.FindAsync<Case>(combinedFilter);
            return specificCase.ToList();
        }

        public async Task<List<Case>> GetCasesOfCaseManager(string caseManagerId, Filters filters)
        {
            ObjectId CaseManagerObjectId = ObjectId.Parse(caseManagerId);
            FilterDefinition<Case> combinedFilter;

            var filter = Builders<Case>.Filter.Eq(x => x.CaseManagerId, CaseManagerObjectId);
            combinedFilter = GetFilterToApply(filters);
            combinedFilter = combinedFilter != null ? Builders<Case>.Filter.And(combinedFilter, filter) : filter;
            var specificCase = await _caseCollection.FindAsync<Case>(filter);
            return specificCase.ToList();
        }
    }
}
