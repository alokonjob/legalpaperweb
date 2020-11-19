using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Audit
{
    public interface IOrderAuditService
    {
        Task<ObjectId> AddAudit(OrderAudit orderAudit);
        Task UpdateAudit(string receipt, List<string> history, bool isCompleted);
    }
}