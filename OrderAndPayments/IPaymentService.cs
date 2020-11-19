using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderAndPayments
{
    public interface IPaymentService
    {
        Task<ClientelePayment> GetPaymentByOrderId(string OrderId);
        Task<List<ClientelePayment>> GetPaymentByOrderId(List<ObjectId> orderIds);
        Task<ObjectId> SavePayment(ClientelePayment clientsPayment);
        void VerifyPayment(IGateWaysPaymentInfo GateWayPaymentDetails);
        Task<ClientelePayment> UpdatePayment(ObjectId paymentId, ObjectId orderId, string status);
        string GetPaymentStatusFromPaymentGateWay(ClientelePayment clientsPayment);


    }
}