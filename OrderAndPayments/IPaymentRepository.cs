using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace OrderAndPayments
{
    public interface IPaymentRepository
    {
        Task<ClientelePayment> GetPaymentByOrderId(string OrderId);
        Task<List<ClientelePayment>> GetPaymentByOrderId(List<ObjectId> orderIds);
        Task<ObjectId> SavePaymentAsync(ClientelePayment clientPayment);
        Task<ClientelePayment> UpdatePayment(ObjectId paymentId, ObjectId OrderId, string status);
    }
}
