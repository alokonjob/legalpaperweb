using CaseManagementSpace;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseManagementSpace
{
    public interface ICaseUpdateService
    {
        Task<CaseUpdate> AddUpdate(CaseUpdate caseupdate);
        Task<List<CaseUpdate>> GetAllUpdates(string caseId);

        Task<List<CaseUpdate>> GetMyUpdates(string caseId, string Email);
    }
}