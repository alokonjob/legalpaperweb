using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace OrderAndPayments
{
    public interface IPaymentRepository
    {
        Task<ClientelePayment> GetPaymentByOrderId(string OrderId);
        Task<ClientelePayment> GetPaymentByCaseId(string CaseId);
        Task<List<ClientelePayment>> GetPaymentByOrderId(List<ObjectId> orderIds);
        Task<ObjectId> SavePaymentAsync(ClientelePayment clientPayment);
        Task<ClientelePayment> UpdatePayment(ObjectId paymentId, ObjectId OrderId, ObjectId CaseId, string status);
        Task<ClientelePayment> UpdatePaymentLinkAsync(ClientelePayment clientPayment);
    }
}
