using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseManagement
{
    public interface ICasePaymentReleaseService
    {
        Task<PayToConsultant> GetPaymentsForCase(string caseId, string ConsultantId);
        Task<List<PayToConsultant>> GetPaymentsForCases(List<ObjectId> cases);
        Task<PayToConsultant> ReleasePayment(string caseId, string ConsultantId, PaymentReleaseInfo payMetadata);
        Task<PayToConsultant> SetFinalizedCost(string caseId, string ConsultantId, double FinalizedCost);
    }
}