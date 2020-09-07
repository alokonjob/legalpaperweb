using System;

namespace OrderAndPayments
{
    public class ClientRefund
    { 
        public DateTime RefundInitiateOn { get; set; }
        public string RefundReason { get; set; }
        public string PaymentGateWay_RefundId { get; set; }
    }
}
