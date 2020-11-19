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
        Task<List<Case>> GetCasesOfCaseManager(string caseManagerId);
    }

    public interface ICaseRepository : IConsultantCaseRepository , ICaseManagerCaseRepository
    {
        Task<ObjectId> AddCase(Case clientCase);
        Task<List<Case>> GetAll();
        Task<List<Case>> GetAllCasesOfUser(string userEmail);

        Task<Case> GetCaseById(string caseId);
        Task<Case> GetCaseByReceipt(string receipt);
        Task<Case> UpdateConsultant(Case caseToUpdate);


    }
}