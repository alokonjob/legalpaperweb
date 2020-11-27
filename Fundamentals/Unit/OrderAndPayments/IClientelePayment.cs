using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;

namespace OrderAndPayments
{
    public interface IClientelePayment
    {
        ObjectId ClienteleOrderId { get; set; }
        IGateWaysPaymentInfo GateWayDetails { get; set; }
        double FinalAmount { get; set; }
        DateTime PaymentDate { get; set; }
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        ObjectId PaymentId { get; set; }
        List<ClientRefund> RefundDetails { get; set; }
        /// <summary>
        /// For all Failed Payments we need to run a job to check with razoay and 
        /// update their status it is TODO work
        /// </summary>
        

    }
}
