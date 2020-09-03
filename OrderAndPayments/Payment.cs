using Fundamentals.Unit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;

namespace OrderAndPayments
{
    public interface IClientelePayment
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        ObjectId ClienteleOrderId { get; set; }
        IGateWaysPaymentInfo GateWayDetails { get; set; }
        double FinalAmount { get; set; }
        DateTime PaymentDate { get; set; }
        ObjectId PaymentId { get; set; }
        ClientRefund RefundDetails { get; set; }
        /// <summary>
        /// For all Failed Payments we need to run a job to check with razoay and 
        /// update their status it is TODO work
        /// </summary>
        

    }

    public class ClientelePayment :  IClientelePayment
    {

        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId PaymentId { get; set; }
        public IGateWaysPaymentInfo GateWayDetails { get; set; }
        public double FinalAmount { get; set; }
        public ObjectId ClienteleOrderId { get; set; }
        public ClientRefund RefundDetails { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
    }

    public interface IGateWaysPaymentInfo
    {
        string PaymentGateWay_OrderId { get; set; }
        string PaymentGateWay_PayId { get; set; }
        string PaymentGateWay_Signature { get; set; }
        string PaymentGateWayName { get;}
    }

    public class RazorPePaymentDetails : IGateWaysPaymentInfo
    {
        public string PaymentGateWayName => "RazorPay";
        public string PaymentGateWay_OrderId { get ; set; }
        public string PaymentGateWay_PayId { get; set; }
        public string PaymentGateWay_Signature { get; set; }
    }


    public class ClientRefund
    { 
        public DateTime RefundInitiateOn { get; set; }
        public string RefundReason { get; set; }
        public string PaymentGateWay_RefundId { get; set; }
    }

    public class ClienteleOrder
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId ClientOrderId { get; set; }
        public EnabledServices CustomerRequirementDetail { get; set; }
        public string Receipt { get; set; }
        public Object ClientelePaymentId { get; set; }
        public DateTime OrderPlacedOn { get; set; }
        

    }

    public class ContactDetails
    { 
        public ObjectId CustomerId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }



    public interface IPaymentGateWayInterface
    { 
        
    }
}
