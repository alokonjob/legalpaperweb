using Fundamentals.Unit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;

namespace OrderAndPayments
{
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

    }


    public class AbridgedOrder
    { 
        public string ServiceName { get; set; }
        public string City { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }

        public string ConsultantEmail { get; set; }

        public string ConsultantPhone { get; set; }
        public string CostToCustomer { get; set; }
        
    }
}
