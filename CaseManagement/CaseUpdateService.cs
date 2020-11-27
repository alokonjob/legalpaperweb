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

        public async Task<List<CaseUpdate>> GetAllUpdates(string caseId, bool includeDeleted = false)
        {
            return await caseUpdateRepository.GetAllUpdates(caseId, includeDeleted);
        }
        public async Task<List<CaseUpdate>> GetMyUpdates(string caseId,string email)
        {
            return await caseUpdateRepository.GetMyUpdates(caseId, email);
        }


        public async Task<CaseUpdate> AddUpdate(CaseUpdate caseupdate)
        {
            return await caseUpdateRepository.PostUpdate(caseupdate);
        }

        public async Task<CaseUpdate> RemoveUpdate(string updateId)
        {
            return await caseUpdateRepository.RemoveUpdate(updateId);
        }
    }
}
