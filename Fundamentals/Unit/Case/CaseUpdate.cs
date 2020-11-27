using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using Users;

namespace CaseManagementSpace
{
    public class CaseUpdate
    {
        [BsonId]
        public ObjectId CaseUpdateId { get; set; }
        public ObjectId CaseId { get; set; }
        public List<int> AttachmentIds { get; set; }
        public string Comment { get; set; }
        public AbridgedUser UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UserFriendlyUpdateDate => UpdatedDate.ToLongDateString() +  UpdatedDate.ToLongTimeString();
        public string ShareWithConsultantEmail { get; set; }

        public bool IsDeleted { get; set; }

    }

    public class AllCaseUpdate
    {
        public CaseUpdate Update { get; set; }
        public bool DELETEId { get; set; }
    }
}
