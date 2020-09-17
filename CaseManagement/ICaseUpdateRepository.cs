using CaseManagementSpace;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseManagementSpace
{
    public interface ICaseUpdateRepository
    {
        Task<List<CaseUpdate>> GetAllUpdates(string caseId);
        Task<CaseUpdate> PostUpdate(CaseUpdate newUpdate);
    }
}