using Fundamentals.Managers;
using Fundamentals.Unit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Consultant
{
    public class ConsultantCareerManagement : IConsultantCareerManagement
    {
        private readonly IConsultantCareerRepository consultantCareerRepository;
        private readonly IEnabledServices enabledServices;

        public ConsultantCareerManagement(IConsultantCareerRepository consultantCareerRepository,IEnabledServices enabledServices)
        {
            this.consultantCareerRepository = consultantCareerRepository;
            this.enabledServices = enabledServices;
        }

        public async Task<ConsultantCareer> IntroduceConsultantCareer(ConsultantCareer consultantsCareer)
        {
            return await consultantCareerRepository.AddConsultantCareer(consultantsCareer);
        }

        public async Task<ConsultantVerificationDetails> AddConsultantDocuments(ConsultantVerificationDetails consultantsDocuments)
        {
            return await consultantCareerRepository.AddVerificationDetails(consultantsDocuments);
        }

        public async Task<ConsultantTaxDetails> AddConsultantTaxDetails(ConsultantTaxDetails taxDetails)
        {
            return await consultantCareerRepository.AddTaxDetails(taxDetails);
        }


        public async Task<List<ConsultantCareer>> GetConsultantForEnabledService(string EnabledServiceId)
        {
            return await consultantCareerRepository.GetConsultantsForService(EnabledServiceId);
        }

        public async Task<List<ConsultantCareer>> GetConsultantForEnabledService(string serviceName ,string city)
        {
            EnabledServices service =  enabledServices.GetEnabledService(serviceName, city);
            return await consultantCareerRepository.GetConsultantsForService(service.EnableId);
        }
    }
}
