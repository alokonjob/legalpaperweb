using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using Razorpay.Api;
using RestSharp;
using RestSharp.Authenticators;

namespace OrderAndPayments
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository PaymentRepository;
        private readonly ILogger<PaymentService> logger;

        public PaymentService(IPaymentRepository PaymentRepository,ILogger<PaymentService> logger)
        {
            this.PaymentRepository = PaymentRepository;
            this.logger = logger;
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

        public async Task<ClientelePayment> UpdatePaymentLinkAsync(ClientelePayment clientPayment)
        {
            return await PaymentRepository.UpdatePaymentLinkAsync(clientPayment);
        }

        public async Task<ClientelePayment> UpdatePayment(ObjectId paymentId, ObjectId orderId, ObjectId caseId, string status)
        {
            return await PaymentRepository.UpdatePayment(paymentId, orderId, caseId,status);
        }

        public async Task<ClientelePayment> GetPaymentByOrderId(string OrderId)
        {
            return await PaymentRepository.GetPaymentByOrderId(OrderId);
        }

        public async Task<Dictionary<string, string>> GeneratePaymentLink(Dictionary<string, string> paymentData)
        {
            try
            {
                var client = new RestSharp.RestClient("https://api.razorpay.com/v1/payment_links/");

                client.Authenticator = new HttpBasicAuthenticator("rzp_test_ju6u0OTTuolb5J", "mUb1k41FXOvU9qrCFAyqQAY4");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                client.Execute(request);
                foreach (var key in paymentData.Keys)
                {
                    request.AddHeader(key, paymentData[key]);
                }
                var response = await client.ExecuteAsync(request);
            }
            catch (Exception)
            {

            }
            return paymentData;
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
            try
            {
                string paymentId = clientsPayment.GateWayDetails.PaymentGateWay_PayId;
                logger.LogInformation($"Fetch Payment for Payment.{clientsPayment.PaymentId}.GateWayPayment.{paymentId}");
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
            }
            catch (Exception error)
            {
                logger.LogError($"Unable To Fetch Payment Status - {error.Message}");
            }
            return string.Empty;
        }
    }
}
