using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
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

        public async Task<ClientelePayment> UpdatePayment(ObjectId paymentId, ObjectId orderId, ObjectId caseId, string status)
        {
            return await PaymentRepository.UpdatePayment(paymentId, orderId, caseId,status);
        }

        public async Task<ClientelePayment> GetPaymentByOrderId(string OrderId)
        {
            return await PaymentRepository.GetPaymentByOrderId(OrderId);
        }


        public async Task<ClientelePayment> GetPaymentByCaseId(string caseId)
        {
            return await PaymentRepository.GetPaymentByCaseId(caseId);
        }

        public async Task<List<ClientelePayment>> GetPaymentByOrderId(List<ObjectId> orderIds)
        {
            return await PaymentRepository.GetPaymentByOrderId(orderIds);
        }

        public string GetPaymentStatusFromPaymentGateWay(ClientelePayment clientsPayment)
        {
            string paymentId = clientsPayment.GateWayDetails.PaymentGateWay_PayId;
            Payment payInfoAtRazor = new Razorpay.Api.Payment((string)paymentId).Fetch(paymentId);
            string paymentStatus = string.Empty;
            foreach (var child in payInfoAtRazor.Attributes)
            {
                if (child.Type == JTokenType.Property)
                {
                    var property = child as Newtonsoft.Json.Linq.JProperty;
                    if (property.Name == "status")
                    {
                        return property.Value.ToString();
                    }
                }
            }
            return string.Empty;
        }
    }
}
