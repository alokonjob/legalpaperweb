using Fundamentals.Unit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Consultant
{
    public interface IConsultantCareerRepository
    {
        Task<ConsultantCareer> AddConsultantCareer(ConsultantCareer consultantCareer);
        Task<ConsultantVerificationDetails> AddVerificationDetails(ConsultantVerificationDetails docs);
        Task<ConsultantTaxDetails> AddTaxDetails(ConsultantTaxDetails docs);
        Task<List<ConsultantCareer>> GetConsultantsForService(string enabledServiceId);
    }
}