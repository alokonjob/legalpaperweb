using Fundamentals.Unit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.ComponentModel;

namespace OrderAndPayments
{
    public enum OrderStatus
    {
        Initiated = 0,

        PaymentStarted = 10,
        WaitingForCustomerPayment = 11,

        PaymentCompletedSuccess = 20,

        PaymentCompletedFailure = 30,

        [Description("Order Successful")]
        OrderCompletedSuccess = 40,

        OrderCompletedFailure = 50
    }

    public enum OrderType
    {
        FreeCallBack = 0,
        PaidCallBack = 1,

        CustomOrder = 100,
        Consultancy = 200,
        RegularOrder = 300
    }
    public class ClienteleOrder
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId ClientOrderId { get; set; }
        public EnabledServices CustomerRequirementDetail { get; set; }
        public string Receipt { get; set; }
        public Object ClientelePaymentId { get; set; }
        public DateTime OrderPlacedOn { get; set; }
        public ObjectId ClientId { get; set; }
        public ObjectId CaseId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string CreatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public OrderType Type { get; set; }
        public ObjectId LinkOrderId { get; set; }

    }


    public class AbridgedOrder
    {
        public ObjectId OrderId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDisplayName { get; set; }
        public string City { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }

        public string ConsultantEmail { get; set; }

        public string ConsultantPhone { get; set; }
        public string CostToCustomer { get; set; }
        [BsonIgnore]
        public string CustomerName { get; set; }
        public string Receipt { get; set; }
        public EnableServiceType CallbackType { get; set; }
        public OrderType OrderType { get; set; }
        public string Link { get; set; }
    }

    public static class PaymentTypeExtension
    {
        public static PaymentType GetPaymentType(this ClienteleOrder clientOrder)
        {
            return GetPaymentType(clientOrder.Type);
        }

        public static PaymentType GetPaymentType(this AbridgedOrder clientOrder)
        {
            return GetPaymentType(clientOrder.OrderType);
        }

        private static PaymentType GetPaymentType(OrderType typeOfOrder)
        {
            PaymentType typeOfPayment;
            switch (typeOfOrder)
            {
                case OrderType.RegularOrder:
                    {
                        typeOfPayment = PaymentType.GateWay;
                    }
                    break;
                case OrderType.FreeCallBack:
                    {
                        typeOfPayment = PaymentType.Free;
                    }
                    break;
                case OrderType.PaidCallBack:
                    {
                        typeOfPayment = PaymentType.GateWay;
                    }
                    break;
                case OrderType.CustomOrder:
                    {
                        typeOfPayment = PaymentType.PaymentLink;
                    }
                    break;
                default:
                    {
                        typeOfPayment = PaymentType.GateWay;
                    }
                    break;
            }
            return typeOfPayment;
        }
    }
}
