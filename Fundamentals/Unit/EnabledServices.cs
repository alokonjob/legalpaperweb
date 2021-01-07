using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals.Unit
{
    /// <summary>
    /// This class is a map of Service and Geography
    /// </summary>
    public class EnabledServices
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string EnableId { get; set; }
        public Service ServiceDetail { get; set; }
        public Geography Location { get; set; }
        public List<ServiceStep> Steps { get; set; }
        public bool IsActive { get; set; }
        public double CostToCustomer { get; set; }
        public double CostToConsultant { get; set; }
        public EnableServiceType KindofService { get; set; }
    }

    public class ServiceStep
    { 
        public string Name { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
    }

    public enum EnableServiceType  
    { 
        None = -1,
        Individual = 0 ,
        Corporate = 1
    }
}
