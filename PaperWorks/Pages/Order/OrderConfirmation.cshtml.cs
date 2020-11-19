using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CaseManagementSpace;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrderAndPayments;

namespace PaperWorks
{
    public class OrderConfirmationModel : PageModel
    {
        private readonly IOrderService orderService;
        private readonly IPaymentService paymentService;
        private readonly ICaseManagement caseManagementService;

        public OrderConfirmationModel(IOrderService orderService, IPaymentService paymentService, ICaseManagement caseManagementService)
        {
            this.orderService = orderService;
            this.paymentService = paymentService;
            this.caseManagementService = caseManagementService;
        }
        [TempData]
        public string CustomerOrderId { get; set; }
        public string PaymentStatus { get; set; }
        public string CustomerRecceiptId { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerFinalPaidAmount { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            var orderReceipt = HttpContext.Session.GetString("OrderReceipt");
            var customerOrder =  await orderService.GetOrderByReceipt(orderReceipt);
            var paymentDetails = paymentService.GetPaymentByOrderId(customerOrder.ClientOrderId.ToString());

            var order = customerOrder;
            var payment = await paymentDetails;
            PaymentStatus = CustomerPaymentMessages(payment.PaymentStatus);// string.Compare(payment.PaymentStatus, "captured", true) == 0 ? "Completed" : "Not Confirmed";
            var caseDetails = await caseManagementService.GetCaseById(order.CaseId.ToString());
            CustomerEmail = caseDetails.Order.CustomerEmail;
            CustomerPhone = caseDetails.Order.CustomerPhone;
            CustomerRecceiptId = order.Receipt;
            double paymentInRupee = payment.FinalAmount;

            CustomerFinalPaidAmount = paymentInRupee.ToString("#,##0.00");
            return Page();
        }

        private string CustomerPaymentMessages(string paymentStatusFromRazor)
        {
            if (string.Compare(paymentStatusFromRazor, "captured", true) == 0)
            { return "Completed"; }
            else
            {
                ModelState.AddModelError(string.Empty,"We have not received payment confirmation yet. Dont Worry ! We are confirming with RazorPay. In case money is deducted but not received by us it shall be refunded");
                return "Not Confirmed";
            }
        }
    }
}