using CaseManagementSpace;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseManagementSpace
{
    public interface ICaseUpdateService
    {
        Task<CaseUpdate> AddUpdate(CaseUpdate caseupdate);
        Task<List<CaseUpdate>> GetAllUpdates(string caseId, bool includeDeleted = false);

        Task<CaseUpdate> RemoveUpdate(string updateId);

        Task<List<CaseUpdate>> GetMyUpdates(string caseId, string Email);
    }
}