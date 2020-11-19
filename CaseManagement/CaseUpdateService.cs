using CaseManagementSpace;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CaseManagementSpace
{
    public class CaseUpdateService : ICaseUpdateService
    {
        private readonly ICaseUpdateRepository caseUpdateRepository;

        public CaseUpdateService(ICaseUpdateRepository caseUpdateRepository)
        {
            this.caseUpdateRepository = caseUpdateRepository;
        }

        public async Task<List<CaseUpdate>> GetAllUpdates(string caseId)
        {
            return await caseUpdateRepository.GetAllUpdates(caseId);
        }
        public async Task<List<CaseUpdate>> GetMyUpdates(string caseId,string email)
        {
            return await caseUpdateRepository.GetMyUpdates(caseId, email);
        }


        public async Task<CaseUpdate> AddUpdate(CaseUpdate caseupdate)
        {
            return await caseUpdateRepository.PostUpdate(caseupdate);
        }
    }
}
