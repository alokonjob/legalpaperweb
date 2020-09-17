using Fundamentals.Unit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Consultant
{
    public interface IConsultantCareerManagement
    {
        Task<ConsultantVerificationDetails> AddConsultantDocuments(ConsultantVerificationDetails consultantsDocuments);
        Task<ConsultantTaxDetails> AddConsultantTaxDetails(ConsultantTaxDetails taxDetails);
        Task<ConsultantCareer> IntroduceConsultantCareer(ConsultantCareer consultantsCareer);
        Task<List<ConsultantCareer>> GetConsultantForEnabledService(string EnabledServiceId);
        Task<List<ConsultantCareer>> GetConsultantForEnabledService(string serviceName, string city);
    }
}