namespace OrderAndPayments
{
    public class RazorPePaymentDetails : IGateWaysPaymentInfo
    {
        public string PaymentGateWayName => "RazorPay";
        public string PaymentGateWay_OrderId { get ; set; }
        public string PaymentGateWay_PayId { get; set; }
        public string PaymentGateWay_Signature { get; set; }
    }
}
