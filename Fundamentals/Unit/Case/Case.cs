using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using OrderAndPayments;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaseManagementSpace
{
    public enum CaseStatus
    { 
        None = 0,
        InProgress =10,
        Critical = 20,
        Closed = 100,
    }

    public enum CaseEscalationStatus
    { 
        Yellow = 10,
        Orange = 20,
        Red = 30
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
        public DateTime ExpectedCaseCloseDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<CaseUpdate> CaseUpdates { get; set; }

        public CaseStatus CurrentStatus { get; set; }

        public CaseEscalationStatus EscalationStatus {get;set;}

    }


    public class CaseUpdate
    {
        public ObjectId CaseId { get; set; }
        public List<int> AttachmentIds { get; set; }
        public string Comment { get; set; }
        public ObjectId UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }

    }
}
