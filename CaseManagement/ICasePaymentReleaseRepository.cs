using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseManagement
{
    public interface ICasePaymentReleaseRepository
    {
        Task<PayToConsultant> GetPaymentsForCase(string caseId, string ConsultantId);
        Task<List<PayToConsultant>> GetPaymentsForCases(List<ObjectId> cases);
        Task<PayToConsultant> ReleasePayment(string caseId, string ConsultantId, PaymentReleaseInfo payment);
        Task<PayToConsultant> SetFinalizedCost(string caseId, string ConsultantId, double FinalizedCost);
        
    }

}
