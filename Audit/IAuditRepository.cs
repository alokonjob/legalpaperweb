using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Audit
{
    public interface IOrderAuditRepository
    {
        Task<ObjectId> AddAudit(OrderAudit audit);
        Task UpdateAudit(string receipt, List<string> audit, bool isCompleted);
    }
}