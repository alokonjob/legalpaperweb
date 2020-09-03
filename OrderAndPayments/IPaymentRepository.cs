using MongoDB.Bson;
using System;
using System.Threading.Tasks;
namespace OrderAndPayments
{
    public interface IPaymentRepository
    {
        Task<ObjectId> SavePaymentAsync(ClientelePayment clientPayment);
        Task<ClientelePayment> UpdatePayment(ObjectId paymentId, ObjectId OrderId, string status);
    }
}
