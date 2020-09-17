using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CaseManagementSpace
{
    public class CaseManagement : ICaseManagement
    {
        private readonly ICaseRepository caseRepository;

        public CaseManagement(ICaseRepository caseRepository)
        {
            this.caseRepository = caseRepository;
        }
        public async Task<ObjectId> GenerateCase(Case customerCase)
        {
            return await caseRepository.AddCase(customerCase);
        }

        public async Task<List<Case>> GetAllCases()
        {
            return await caseRepository.GetAll();
        }

        public async Task<Case> GetCaseById(string caseId)
        {
            return await caseRepository.GetCaseById(caseId);
        }

        public async Task<Case> UpdateConsultant(Case caseToUpdate)
        {
            return await caseRepository.UpdateConsultant(caseToUpdate);
        }
    }
}
