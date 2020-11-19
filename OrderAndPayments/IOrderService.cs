using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderAndPayments
{
    public interface IOrderService
    {
        Task<ClienteleOrder> SaveOrder(ClienteleOrder order);
        Task<ClienteleOrder> AddPaymentToOrder(string orderId, string paymentId);
        Task<ClienteleOrder> AddCaseToOrder(ObjectId order, ObjectId caseId);
        Task<ClienteleOrder> GetOrderByCaseId(string caseId);
        Task<ClienteleOrder> GetOrderById(string orderId);
        Task<ClienteleOrder> GetOrderByReceipt(string receipt);
        Task<List<ClienteleOrder>> GetOrdersOFUser(string userId);
    }
}