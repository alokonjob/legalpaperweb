using CaseManagementSpace;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseManagementSpace
{
    public interface ICaseUpdateRepository
    {
        Task<List<CaseUpdate>> GetAllUpdates(string caseId,bool includeDeleted = false);
        Task<List<CaseUpdate>> GetMyUpdates(string caseId, string Email);
        Task<CaseUpdate> PostUpdate(CaseUpdate newUpdate);
        Task<CaseUpdate> RemoveUpdate(string updateId);
    }
}