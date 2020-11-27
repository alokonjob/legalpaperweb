using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseManagementSpace
{
    public interface IConsultantCaseManagement
    {
        Task<List<Case>> GetAllCasesOfConsultant(string userEmail);
    }

    public interface ICaseManagerCaseManagement
    {
        Task<List<Case>> GetAllCasesOfCaseManager(string userEmail);
    }
    public interface ICaseManagement:IConsultantCaseManagement, ICaseManagerCaseManagement
    {
        Task<ObjectId> GenerateCase(Case customerCase, Dictionary<string, string> caseDictionary);
        Task<List<Case>> GetAllCases();
        Task<List<Case>> GetAllCasesOfUser(string userEmail);
        Task<Case> GetCaseById(string caseId);
        Task<Case> GetCaseByReceipt(string receipt);
        Task<Case> UpdateConsultant(Case caseToUpdate);
    }
}