using Fundamentals.Unit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;

namespace OrderAndPayments
{
    public enum OrderStatus
    { 
        Initiated =0,

        PaymentStarted = 10,

        PaymentCompletedSuccess = 20,

        PaymentCompletedFailure = 30,

        OrderCompletedSuccess = 40,

        OrderCompletedFailure = 50
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

    }


    public class AbridgedOrder
    {
        public ObjectId OrderId { get; set; }
        public string ServiceName { get; set; }
        public string City { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }

        public string ConsultantEmail { get; set; }

        public string ConsultantPhone { get; set; }
        public string CostToCustomer { get; set; }
        [BsonIgnore]
        public string CustomerName { get; set; }
        public string Receipt { get; set; }

    }
}
