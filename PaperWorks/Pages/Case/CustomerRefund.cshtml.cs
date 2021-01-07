using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaseManagementSpace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using OrderAndPayments;
using Razorpay.Api;

namespace PaperWorks
{

    public class FullRefundInformation
    {
        public List<Dictionary<string, string>> REFUNDLISTFROMRAZOR = new List<Dictionary<string, string>>();
        public double Total { get; set; }
    }
    public class CustomerRefundModel : PageModel
    {
        private readonly ICaseManagement caseManagement;
        private readonly IPaymentService paymentService;

        [BindProperty]
        public ClientRefund InputForRefund { get; set; }
        [BindProperty(SupportsGet = true)]
        public string Receipt { get; set; }
        public ClientelePayment CustomerPayment { get; set; }
        public List<Refund> FullRefundInfo { get; set; }
        public string TextMessage { get; set; }
        public CustomerRefundModel(ICaseManagement caseManagement, IPaymentService paymentService)
        {
            this.caseManagement = caseManagement;
            this.paymentService = paymentService;
        }

        [BindProperty]
        public FullRefundInformation RefundInformationContainer { get; set; }

        public async Task<IActionResult> OnGet(string receipt)
        {
            var caseByReceipt = caseManagement.GetCaseByReceipt(receipt);

            await paymentService.GetPaymentByCaseId(caseByReceipt.Result.CaseId.ToString());
            CustomerPayment = await paymentService.GetPaymentByCaseId(caseByReceipt.Result.CaseId.ToString());
            TextMessage = GetMessageForRefund(CustomerPayment);
            if (!string.IsNullOrEmpty(TextMessage))
            {
                ModelState.AddModelError(string.Empty, TextMessage);
            }
            RazorpayClient client = new RazorpayClient("rzp_test_ju6u0OTTuolb5J", "mUb1k41FXOvU9qrCFAyqQAY4");
            try
            {
                var payment = client.Payment.Fetch(CustomerPayment.GateWayDetails.PaymentGateWay_PayId);

                FullRefundInfo = payment.AllRefunds();
            }
            catch (Exception error)
            {
                FullRefundInfo = new List<Refund>();
            }

            double total = 0.0;

            RefundInformationContainer = new FullRefundInformation();
            foreach (var refundInfo in FullRefundInfo)
            {
                Dictionary<string, string> REFUNDDATAFROMRAZOR = new Dictionary<string, string>();
                foreach (var child in refundInfo.Attributes)
                {
                    if (child.Type == JTokenType.Property)
                    {
                        var property = child as Newtonsoft.Json.Linq.JProperty;
                        REFUNDDATAFROMRAZOR.Add(property.Name, property.Value.ToString());
                        if (property.Name == "amount")
                        {
                            total += Convert.ToDouble(property.Value.ToString());
                        }
                    }
                }
                RefundInformationContainer.REFUNDLISTFROMRAZOR.Add(REFUNDDATAFROMRAZOR);
            }
            RefundInformationContainer.Total = total/100;

            return Page();
        }

        private string GetMessageForRefund(ClientelePayment customerPayment)
        {
            switch (customerPayment.PaymentType)
            { 
               case PaymentType.Free:
                    {
                        return $"Refund Not Needed , Payment Was Free";
                    }
                case PaymentType.PaymentLink:
                    {
                        return $"Refund To Be Generated from Razor, not here";
                    }

            }
            return string.Empty;
        }

        public async Task<PartialViewResult> OnPostRefundPaymentAsync()
        {
            try
            {
                var caseByReceipt = caseManagement.GetCaseByReceipt(Receipt);
                var customerPayment = await paymentService.GetPaymentByCaseId(caseByReceipt.Result.CaseId.ToString());

                CustomerPayment = await paymentService.GetPaymentByCaseId(caseByReceipt.Result.CaseId.ToString());
                if (CustomerPayment.PaymentType == PaymentType.Free || CustomerPayment.PaymentType == PaymentType.PaymentLink)
                {
                    return Partial("_RefundInformation", null);
                }
                RazorpayClient client = new RazorpayClient("rzp_test_ju6u0OTTuolb5J", "mUb1k41FXOvU9qrCFAyqQAY4");
                var rzorPayment = client.Payment.Fetch(CustomerPayment.GateWayDetails.PaymentGateWay_PayId);
                //CustomerPaymentWithRazor = new Razorpay.Api.Payment(CustomerPayment.GateWayDetails.PaymentGateWay_PayId).Fetch(CustomerPayment.GateWayDetails.PaymentGateWay_PayId);
                if (customerPayment.FinalAmount < InputForRefund.RefundAmount)
                {
                    ModelState.AddModelError(string.Empty, $"Amount can not be greater than customer payment amount {InputForRefund.RefundAmount}");
                }
                Dictionary<string, object> data = new Dictionary<string, object>();
                data.Add("amount", (InputForRefund.RefundAmount * 100).ToString());
                Refund refund = rzorPayment.Refund(data);
                //return Partial("_RefundInformation", refund);
                double total = 0.0;
                FullRefundInfo = rzorPayment.AllRefunds();

                RefundInformationContainer = new FullRefundInformation();


                foreach (var refundInfo in FullRefundInfo)
                {
                    Dictionary<string, string> REFUNDDATAFROMRAZOR = new Dictionary<string, string>();
                    foreach (var child in refundInfo.Attributes)
                    {
                        if (child.Type == JTokenType.Property)
                        {
                            var property = child as Newtonsoft.Json.Linq.JProperty;
                            REFUNDDATAFROMRAZOR.Add(property.Name, property.Value.ToString());
                            if (property.Name == "amount")
                            {
                                total += Convert.ToDouble(property.Value.ToString());
                            }
                        }
                    }
                    RefundInformationContainer.REFUNDLISTFROMRAZOR.Add(REFUNDDATAFROMRAZOR);
                }
                RefundInformationContainer.Total = total/100;


                return Partial("_RefundInformation", RefundInformationContainer);

            }
            catch (Exception error)
            {

            }
            return Partial("_RefundInformation", null);
        }

    }
}