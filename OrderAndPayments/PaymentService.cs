using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Razorpay.Api;
namespace OrderAndPayments
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository PaymentRepository;

        public PaymentService(IPaymentRepository PaymentRepository)
        {
            this.PaymentRepository = PaymentRepository;
        }
        public void VerifyPayment(IGateWaysPaymentInfo paymentDetailsByGateWay)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();

            attributes.Add("razorpay_payment_id", paymentDetailsByGateWay.PaymentGateWay_PayId);
            attributes.Add("razorpay_order_id", paymentDetailsByGateWay.PaymentGateWay_OrderId);
            attributes.Add("razorpay_signature", paymentDetailsByGateWay.PaymentGateWay_Signature);

            Utils.verifyPaymentSignature(attributes);
        }

        public async Task<ObjectId> SavePayment(ClientelePayment clientsPayment)
        {
            return await PaymentRepository.SavePaymentAsync(clientsPayment);
        }

        public async Task<ClientelePayment> UpdatePayment(ObjectId paymentId, ObjectId orderId, string status)
        {
            return await PaymentRepository.UpdatePayment(paymentId, orderId, status);
        }
    }
}
