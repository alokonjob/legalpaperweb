using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseManagementSpace
{
    public interface IConsultantCaseRepository
    {
        Task<List<Case>> GetCasesOfConsultant(string consultantId);
    }

    public interface ICaseManagerCaseRepository
    {
        Task<List<Case>> GetCasesOfCaseManager(string caseManagerId, Filters filters);
    }

    public interface ICaseRepository : IConsultantCaseRepository , ICaseManagerCaseRepository
    {
        Task<ObjectId> AddCase(Case clientCase);
        Task<List<Case>> GetAll(Filters filter = null);
        Task<List<Case>> GetAllCasesOfUser(string userEmail);

        Task<Case> GetCaseById(string caseId);
        Task<Case> GetCaseByReceipt(string receipt);
        Task<Case> UpdateConsultant(Case caseToUpdate);
        Task<Case> UpdateCaseManager(Case caseToUpdate);
        Task<Case> AcceptCase(Case caseToUpdate, string Email);
        Task<Case> ChangeStatus(string receipt, CaseStatus caseStatus);

    }
}