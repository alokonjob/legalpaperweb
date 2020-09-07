using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseManagementSpace
{
    public interface ICaseManagement
    {
        Task<ObjectId> GenerateCase(Case customerCase);
        Task<List<Case>> GetAllCases();
    }
}