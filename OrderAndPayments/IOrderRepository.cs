using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderAndPayments
{
    public interface IOrderRepository
    {
        Task<ClienteleOrder> Add(ClienteleOrder order);
        Task<ClienteleOrder> AddCaseToOrder(ObjectId orderId, ObjectId CaseId);
        Task<ClienteleOrder> AddPaymentToOrder(string orderId, string paymentId, OrderStatus status = OrderStatus.PaymentCompletedSuccess);
        Task<ClienteleOrder> GetOrderByCaseId(string orderId);
        Task<ClienteleOrder> GetOrderById(string orderId);
        Task<ClienteleOrder> GetOrderByReceipt(string receipt);
        Task<List<ClienteleOrder>> GetOrderOfUser(string  userId);
        Task<List<ClienteleOrder>> GetCustomOrders(string Email = "");
        Task<ClienteleOrder> UpdateOrderStatus(string orderId, OrderStatus status);
        Task<ClienteleOrder> UpdateOrderStatusByReceipt(string receipt, OrderStatus status);
        Task<ClienteleOrder> UpdateCustomerCost(string orderId, double Cost);
    }
}