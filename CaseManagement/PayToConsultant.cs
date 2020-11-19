using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CaseManagement
{
    public class PayToConsultant
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId Id { get; set; }
        public ObjectId CaseId { get; set; }
        public ObjectId CurrentConsultantId { get; set; }
        public double FinalizedCost { get; set; }
        public double PaymentReleased { get; set; }
        public List<PaymentReleaseInfo> PaymentReleaseInformation { get; set; }
        
    }

    public class PaymentReleaseInfo {
        public string PaymentComments { get; set; }
        public string PaymentIdentifier { get; set; }
        public double Payment { get; set; }
        public DateTime ReleaseOn {get;set;}
    }

    public class CasePaymentReleaseService : ICasePaymentReleaseService
    {
        private readonly ICasePaymentReleaseRepository casePaymentRepo;

        public CasePaymentReleaseService(ICasePaymentReleaseRepository casePaymentRepo)
        {
            this.casePaymentRepo = casePaymentRepo;
        }
        public async Task<PayToConsultant> SetFinalizedCost(string caseId, string ConsultantId, double FinalizedCost)
        {
            return await casePaymentRepo.SetFinalizedCost(caseId, ConsultantId, FinalizedCost);
        }

        public async Task<PayToConsultant> ReleasePayment(string caseId, string ConsultantId, PaymentReleaseInfo Payment)
        {
            return await casePaymentRepo.ReleasePayment(caseId, ConsultantId, Payment);
        }

        public async Task<PayToConsultant> GetPaymentsForCase(string caseId, string ConsultantId)
        {
            return await casePaymentRepo.GetPaymentsForCase(caseId, ConsultantId);
        }

        public async Task<List<PayToConsultant>> GetPaymentsForCases(List<ObjectId> cases)
        {
            return await casePaymentRepo.GetPaymentsForCases(cases);
        }
    }

}
