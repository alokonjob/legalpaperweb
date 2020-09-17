using MongoDB.Bson;
using System.Threading.Tasks;

namespace OrderAndPayments
{
    public interface IOrderService
    {
        Task<ClienteleOrder> SaveOrder(ClienteleOrder order);
        Task<ClienteleOrder> AddCaseToOrder(ObjectId order, ObjectId caseId);
        Task<ClienteleOrder> GetOrderByCaseId(string orderId);
    }
}