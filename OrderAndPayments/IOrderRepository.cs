using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderAndPayments
{
    public interface IOrderRepository
    {
        Task<ClienteleOrder> Add(ClienteleOrder order);
        Task<ClienteleOrder> AddCaseToOrder(ObjectId orderId, ObjectId CaseId);
        Task<ClienteleOrder> AddPaymentToOrder(string orderId, string paymentId);
        Task<ClienteleOrder> GetOrderByCaseId(string orderId);
        Task<ClienteleOrder> GetOrderById(string orderId);
        Task<ClienteleOrder> GetOrderByReceipt(string receipt);
        Task<List<ClienteleOrder>> GetOrderOfUser(string  userId);
    }
}