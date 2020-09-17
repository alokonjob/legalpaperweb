using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseManagementSpace
{
    public interface ICaseManagement
    {
        Task<ObjectId> GenerateCase(Case customerCase);
        Task<List<Case>> GetAllCases();
        Task<Case> GetCaseById(string caseId);
        Task<Case> UpdateConsultant(Case caseToUpdate);
    }
}