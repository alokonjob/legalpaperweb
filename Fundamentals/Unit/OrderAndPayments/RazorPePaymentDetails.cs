using MongoDB.Bson.Serialization.Attributes;

namespace OrderAndPayments
{
    [BsonIgnoreExtraElements]
    public class RazorPePaymentDetails : IGateWaysPaymentInfo
    {
        public RazorPePaymentDetails()
        {

        }
        public string PaymentGateWayName => "RazorPay";
        public string PaymentGateWay_OrderId { get ; set; }
        public string PaymentGateWay_PayId { get; set; }
        public string PaymentGateWay_Signature { get; set; }
    }
}
