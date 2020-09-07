using MongoDB.Bson;
using System.Threading.Tasks;

namespace OrderAndPayments
{
    public interface IOrderRepository
    {
        Task<ClienteleOrder> Add(ClienteleOrder order);
        Task<ClienteleOrder> AddCaseToOrder(ObjectId orderId, ObjectId CaseId);
    }
}