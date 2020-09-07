namespace OrderAndPayments
{
    public interface IGateWaysPaymentInfo
    {
        string PaymentGateWay_OrderId { get; set; }
        string PaymentGateWay_PayId { get; set; }
        string PaymentGateWay_Signature { get; set; }
        string PaymentGateWayName { get;}
    }
}
