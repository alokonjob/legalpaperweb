using System;
using System.ComponentModel.DataAnnotations;

namespace OrderAndPayments
{
    public class ClientRefund
    { 
        public DateTime RefundInitiateOn { get; set; }
        [Required]
        public string RefundReason { get; set; }
        public string PaymentGateWay_RefundId { get; set; }
        [Required]
        public double RefundAmount { get; set; }
    }
}
