using Fundamentals.DbContext;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseManagement
{
    public class CasePaymentReleaseRepository : ICasePaymentReleaseRepository
    {
        IMongoClient client = null;
        private readonly IMongoCollection<PayToConsultant> _casePaymentCollection;

        public CasePaymentReleaseRepository(IMongoDbContext mongoContext)
        {
            client = mongoContext.GetMongoClient();
            _casePaymentCollection = mongoContext.GetCollection<PayToConsultant>(client, "PayToConsultant");
        }

        public async Task<PayToConsultant> SetFinalizedCost(string caseId, string ConsultantId, double FinalizedCost)
        {
            ObjectId CaseObjectId = ObjectId.Parse(caseId);
            ObjectId ConsultantObjectId = ObjectId.Parse(ConsultantId);
            PayToConsultant consultantPayment = new PayToConsultant();

            var filterToCheckExisting = Builders<PayToConsultant>.Filter.Where(x => x.CaseId == CaseObjectId && x.CurrentConsultantId == ConsultantObjectId);
            var paymnetInfo = await _casePaymentCollection.FindAsync<PayToConsultant>(filterToCheckExisting);
            if (paymnetInfo.FirstOrDefault() == null)
            {
                consultantPayment.CurrentConsultantId = ConsultantObjectId;
                consultantPayment.CaseId = CaseObjectId;
                consultantPayment.FinalizedCost = FinalizedCost;
                consultantPayment.PaymentReleased = 0.0;
                consultantPayment.PaymentReleaseInformation = new List<PaymentReleaseInfo>();
                await _casePaymentCollection.InsertOneAsync(consultantPayment);
            }
            else
            {
                //This is to satisfy following scenario
                // A was consultant and had agreed on 5K. He was given payment of 1K then he left the job.
                //Now B is asked to do this job and is paid 1K. Then B leaves
                // There is no consultant so A is asked to continue and he asked 6K
                //So only finalized cost will change but he has already been paid 1K and that should not be forgotten for this case

                consultantPayment.CurrentConsultantId = ConsultantObjectId;
                consultantPayment.CaseId = CaseObjectId;
                consultantPayment.FinalizedCost = FinalizedCost;
                consultantPayment.PaymentReleased = paymnetInfo.FirstOrDefault().PaymentReleased;


                var updatedPaymentInfo = await _casePaymentCollection.FindOneAndUpdateAsync<PayToConsultant>(
                        filterToCheckExisting,
                        Builders<PayToConsultant>.
                        Update
                        .Set(
                            t => t.FinalizedCost,
                            FinalizedCost)
                        );
            }
            return consultantPayment;
        }

        public async Task<PayToConsultant> ReleasePayment(string caseId, string ConsultantId, PaymentReleaseInfo payMetadata)
        {
            try
            {
                ObjectId CaseObjectId = ObjectId.Parse(caseId);
                ObjectId ConsultantObjectId = ObjectId.Parse(ConsultantId);

                var filter = Builders<PayToConsultant>.Filter.Where(x => x.CaseId == CaseObjectId && x.CurrentConsultantId == ConsultantObjectId);
                var paymnetInfo = await _casePaymentCollection.FindAsync<PayToConsultant>(filter);
                var payInfo = paymnetInfo.FirstOrDefault();

                payInfo.PaymentReleased += payMetadata.Payment;

                //var update = Builders<PayToConsultant>.Update.Push<PaymentReleaseInfo>(e => e.PaymentReleaseInformation, new PaymentReleaseInfo() { Payment = payMetadata.Payment, ReleaseOn = DateTime.UtcNow });
                payMetadata.ReleaseOn = DateTime.UtcNow;

                var updatedPaymentInfo = await _casePaymentCollection.FindOneAndUpdateAsync<PayToConsultant>(
                        filter,
                        Builders<PayToConsultant>.
                        Update
                        .Push<PaymentReleaseInfo>(e => e.PaymentReleaseInformation, payMetadata)
                        .Set(
                            t => t.PaymentReleased,
                            payInfo.PaymentReleased)
                        );
                return updatedPaymentInfo;
            }
            catch (Exception error)
            {

                throw;
            }

        }

        public async Task<PayToConsultant> GetPaymentsForCase(string caseId, string ConsultantId)
        {
            ObjectId CaseObjectId = ObjectId.Parse(caseId);
            ObjectId ConsultantObjectId = ObjectId.Parse(ConsultantId);

            var filter = Builders<PayToConsultant>.Filter.Where(x => x.CaseId == CaseObjectId && x.CurrentConsultantId == ConsultantObjectId);
            var paymnetInfo = await _casePaymentCollection.FindAsync<PayToConsultant>(filter);
            return paymnetInfo.FirstOrDefault();
        }

        public async Task<List<PayToConsultant>> GetPaymentsForCases(List<ObjectId> cases)
        {
            var filter = Builders<PayToConsultant>.Filter.In(x => x.CaseId,cases);
            var paymnetInfo = await _casePaymentCollection.FindAsync<PayToConsultant>(filter);
            return await paymnetInfo.ToListAsync();
        }

    }

}
