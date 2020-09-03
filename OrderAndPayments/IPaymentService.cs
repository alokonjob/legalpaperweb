using MongoDB.Bson;
using System;
using System.Threading.Tasks;

namespace OrderAndPayments
{
    public interface IPaymentService
    {
        Task<ObjectId> SavePayment(ClientelePayment clientsPayment);
        void VerifyPayment(IGateWaysPaymentInfo GateWayPaymentDetails);
        Task<ClientelePayment> UpdatePayment(ObjectId paymentId, ObjectId orderId, string status);
    }
}