using Fundamentals.Unit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using OrderAndPayments;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CaseManagementSpace
{
    public enum CaseStatus
    { 
        None = 0,
        [Description("Case Generated")]
        Created = 100,
        [Description("Assigning Consultant")]
        PendingConsultantConfirmation = 200,
        [Description("Work Started")]
        InProgress = 300,
        [Description("Closed")]
        CaseManagerClosed = 400,
        [Description("Closed")]
        FinanceClosed = 500,
        [Description("Closed")]
        Closed = 100,
    }

    public enum CaseEscalationStatus
    { 
        None = 0,
        Okay = 100,
        Slow = 200,
        Critical = 300,
    }
    public class Case
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId CaseId { get; set; }
        public AbridgedOrder Order { get; set; }
        public ObjectId CaseManagerId { get; set; }
        public ObjectId CurrentConsultantId { get; set; }
        public List<ObjectId> PreviousConsultantId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ExpectedCaseCloseDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        //public List<CaseUpdate> CaseUpdates { get; set; }
        public CaseStatus CurrentStatus { get; set; }
        public CaseEscalationStatus EscalationStatus {get;set;}
        public string CaseConfirmedBy { get; set; }
        public string CaseConfirmationCode { get; set; }

    }
    public class Filters
    {
        public string Receipt { get; set; }
        public EnableServiceType ServiceType { get; set; }
        public CaseStatus CaseStatus { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
