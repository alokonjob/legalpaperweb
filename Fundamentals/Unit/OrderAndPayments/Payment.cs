﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;

namespace OrderAndPayments
{

    public class ClientelePayment :  IClientelePayment
    {

        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId PaymentId { get; set; }
        public IGateWaysPaymentInfo GateWayDetails { get; set; }
        public double FinalAmount { get; set; }
        public ObjectId ClienteleOrderId { get; set; }
        public ObjectId CaseId { get; set; }
        public List<ClientRefund> RefundDetails { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
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
