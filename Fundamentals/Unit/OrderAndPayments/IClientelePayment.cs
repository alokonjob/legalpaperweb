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
}
