using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Audit;
using CaseManagementSpace;
using Fundamentals.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using OrderAndPayments;
using Razorpay.Api;
using User;

namespace PaperWorks
{
    public class CustomOrderDetailModel : PageModel
    {
        private readonly IOrderService orderService;
        private readonly IPaymentService paymentService;
        private readonly IClienteleServices clienteleServices;
        private readonly ICaseManagement caseManagement;
        private readonly IOrderAuditService orderAudit;
        private readonly IStringLocalizer<CaseDetailModel> localizer;
        private readonly ILogger<CustomOrderDetailModel> logger;

        public ClienteleOrder Order { get; set; }
        [BindProperty]
        public Dictionary<string, string> PAYDATAFROMRAZOR { get; set; }

        public class Input
        {
            [Required]
            public string PayId { get; set; }
            public string CustomerCost { get; set; }
            public string Receipt { get; set; }
        }

        [BindProperty(SupportsGet = true)]
        public Input PayDetail { get; set; }
        StringBuilder AuditString = new StringBuilder();
        public CustomOrderDetailModel(IOrderService orderService, IPaymentService paymentService, IClienteleServices clienteleServices, ICaseManagement caseManagement,
            IOrderAuditService orderAudit, IStringLocalizer<CaseDetailModel> localizer, ILogger<CustomOrderDetailModel> logger)
        {
            this.orderService = orderService;
            this.paymentService = paymentService;
            this.clienteleServices = clienteleServices;
            this.caseManagement = caseManagement;
            this.orderAudit = orderAudit;
            this.localizer = localizer;
            this.logger = logger;
        }
        public async Task<IActionResult> OnGet(string rct)
        {
            Order = await orderService.GetOrderByReceipt(rct);
            var paymentLinkValue = "";
            if (Order.ClientelePaymentId != null)
            {
                try
                {
                    var paymentSaved = await paymentService.GetPaymentByOrderId(Order.ClientOrderId.ToString());
                    paymentLinkValue = paymentSaved.GateWayDetails.PaymentGateWay_PayId;
                }
                catch (Exception ex)
                { 
                
                }
             }


            PayDetail = new Input() { Receipt = rct, PayId = paymentLinkValue };

            return Page();
        }

        public async Task<IActionResult> OnPostSavePayIdAsync()
        {
            try
            {
                Order = await orderService.GetOrderByReceipt(PayDetail.Receipt);
                var costToCusomer = 0.0;
                if (!string.IsNullOrEmpty(PayDetail.CustomerCost) && (Order.CustomerRequirementDetail.CostToCustomer != Convert.ToDouble(PayDetail.CustomerCost)))
                {
                    //Update Customer Cost
                    costToCusomer = Convert.ToDouble(PayDetail.CustomerCost);
                    Order = await orderService.UpdateCustomerCost(Order.ClientOrderId.ToString(), costToCusomer);
                    logger.LogInformation($"CustomOrderDetail.CustomerCost{PayDetail.CustomerCost}.DifferentFfromOrderCost.{Order.CustomerRequirementDetail.CostToCustomer}");
                    AuditString.AppendLine($"CustomOrderDetail.CustomerCost{PayDetail.CustomerCost}.DifferentFfromOrderCost.{Order.CustomerRequirementDetail.CostToCustomer}");
                }
                else
                {
                    costToCusomer = Order.CustomerRequirementDetail.CostToCustomer;
                }

                var payment = PrepareClientelePayment(PayDetail.PayId, costToCusomer);
                payment.ClienteleOrderId = Order.ClientOrderId;
                if (Order.ClientelePaymentId != null)
                {
                    payment = await paymentService.GetPaymentByOrderId(Order.ClientOrderId.ToString());
                    payment.GateWayDetails.PaymentGateWay_PayId = PayDetail.PayId;
                    payment.FinalAmount = costToCusomer;

                }
                var payId = Order.ClientelePaymentId == null ? paymentService.SavePayment(payment).Result : paymentService.UpdatePaymentLinkAsync(payment).Result.PaymentId;
                AuditString.AppendLine($"CustomOrderDetail.PaymentSaved.{payId}");

                Order.ClientelePaymentId = payId;
                var finalOrder = await orderService.AddPaymentToOrder(Order.ClientOrderId.ToString(), payId.ToString(), OrderStatus.WaitingForCustomerPayment);
                AuditString.AppendLine($"CustomOrderDetail.OrderUpdatedWithPayment.{Order.ClientelePaymentId}");
            }
            catch (Exception error)
            {
                logger.LogCritical($"Unknow Error while Updating PayLink Info in Customer Order {error.Message}");
                AuditString.AppendLine($"Unknow Error while Updating PayLink Info in Customer Order");
            }
            finally
            {
                await orderAudit.UpdateAudit(PayDetail.Receipt, AuditString.ToString().Split('\n').ToList(), false);
            }
            return RedirectToPage("/Order/CustomOrderList");
        }

        public async Task<IActionResult> OnPostdeleteCaseAsync()
        {
            await orderService.UpdateOrderStatusByReceipt(PayDetail.Receipt, OrderStatus.PaymentCompletedFailure);
            return Page();
        }

            public async Task<IActionResult> OnPostPaymentDoneAsync()
        {
            bool OrderFullyCompleted = false;
            try
            {
                var clienteleOrder = await orderService.GetOrderByReceipt(PayDetail.Receipt);
                var payment = await paymentService.GetPaymentByOrderId(clienteleOrder.ClientOrderId.ToString());
                clienteleOrder.ClientelePaymentId = payment.PaymentId;


                /*Generate Case*/
                Case clientCase = new Case();
                var users = await clienteleServices.GetUserByIds(new List<ObjectId>() { clienteleOrder.ClientId });
                var userDetails = users.FirstOrDefault();
                clientCase.Order = new AbridgedOrder()
                {
                    OrderId = clienteleOrder.ClientOrderId,
                    ServiceName = clienteleOrder.CustomerRequirementDetail.ServiceDetail.Name,
                    ServiceDisplayName = clienteleOrder.CustomerRequirementDetail.ServiceDetail.DetailedDisplayInfo.DisplayName,
                    City = clienteleOrder.CustomerRequirementDetail.Location.City,
                    CustomerEmail = userDetails.Email,
                    CustomerPhone = userDetails.PhoneNumber,
                    CostToCustomer = clienteleOrder.CustomerRequirementDetail.CostToCustomer.ToString(),
                    CustomerName = userDetails.FullName,
                    Receipt = clienteleOrder.Receipt,
                    CallbackType = clienteleOrder.CustomerRequirementDetail.KindofService,
                    Link = clienteleOrder.LinkOrderId.ToString()


                };
                clientCase.CreatedDate = clienteleOrder.OrderPlacedOn;
                clientCase.PreviousConsultantId = new List<MongoDB.Bson.ObjectId>();
                clientCase.CurrentStatus = CaseStatus.Created;
                clientCase.EscalationStatus = CaseEscalationStatus.None;

                var confirmcallbackUrl = Url.Page(
                    "/Order/OrderList",
                    pageHandler: null,
                    values: new { receipt = clienteleOrder.Receipt, returnUrl = Url.Content("~/") },
                    protocol: Request.Scheme);

                Dictionary<string, string> itemDictionary = new Dictionary<string, string>();
                itemDictionary.Add("##SERVICE", localizer[clienteleOrder.CustomerRequirementDetail.ServiceDetail.DetailedDisplayInfo.DisplayName]);
                itemDictionary.Add("##ORDERNO", clientCase.Order.Receipt);
                itemDictionary.Add("##CITY", clientCase.Order.City.ToUpper());
                itemDictionary.Add("##NAME", clientCase.Order.CustomerName);
                itemDictionary.Add("##PHONE", clientCase.Order.CustomerPhone);
                itemDictionary.Add("##URL", HtmlEncoder.Default.Encode(confirmcallbackUrl));

                AuditString.AppendLine($"CustomOrderDetail.StartGeneratingCase");


                var caseId = await caseManagement.GenerateCase(clientCase, itemDictionary);

                AuditString.AppendLine($"CustomOrderDetail.CaseGenerated.CaseId.{caseId}");

                await paymentService.UpdatePayment(payment.PaymentId, clienteleOrder.ClientOrderId, caseId, "captured");
                await orderService.UpdateOrderStatus(clienteleOrder.ClientOrderId.ToString(), OrderStatus.PaymentCompletedSuccess);
                AuditString.AppendLine($"CustomOrderDetail.Update CaseId and Order Id to Payment.CaseId");

                logger.LogInformation(LogEvents.OrderSavedInPayment, $"Payment.PaymentUpdated.Success.ForOrderReceipt.{clienteleOrder.Receipt}");
                //AuditString.AppendLine($"Checkout.{LogEvents.OrderSavedInPayment}.{Input.Email}.Success");

                await orderService.AddCaseToOrder(clienteleOrder.ClientOrderId, caseId);
                AuditString.AppendLine($"CustomOrderDetail.Added Case To Order and Updated Order Status");
                OrderFullyCompleted = true;
            }
            catch (Exception error)
            {
                OrderFullyCompleted = false;
                logger.LogCritical(LogEvents.CustomOrderDetailError, error, $"CustomOrderDetail.Error.{error.Message}");
                AuditString.AppendLine($"Checkout.{LogEvents.CheckoutError}.{error.Message}");
            }
            finally
            {

                await orderAudit.UpdateAudit(PayDetail.Receipt, AuditString.ToString().Split('\n').ToList(), OrderFullyCompleted);
            }

            return RedirectToPage("/Order/CustomOrderList");
        }

        public ClientelePayment PrepareClientelePayment(string paymentId, double cost)
        {
            ClientelePayment paymentDone = new ClientelePayment();
            paymentDone.GateWayDetails = new RazorPePaymentDetails()
            {
                PaymentGateWay_OrderId = string.Empty,
                PaymentGateWay_PayId = paymentId,
                PaymentGateWay_Signature = string.Empty
            };
            paymentDone.PaymentType = PaymentType.PaymentLink;
            paymentDone.FinalAmount = cost;
            paymentDone.PaymentDate = DateTime.UtcNow;
            paymentDone.RefundDetails = new List<ClientRefund>();
            return paymentDone;
        }
    }
}