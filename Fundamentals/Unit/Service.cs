using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
namespace Fundamentals.Unit
{
    public class Service
    {
        [BsonId(IdGenerator = typeof(GuidGenerator))]
        public Object ServiceId { get; set; }
        [Required]
        [RegularExpression("^[a-z]+$", ErrorMessage = "only small letters, no space allowed")]
        public string Name { get; set; }
        public ServiceType Type { get; set; }
        public bool IsActive { get; set; }
        public ServiceDetailedInformation DetailedDisplayInfo{get;set;}
    }

    public enum ServiceType
    { 
        Individual,
        Corporate
    }
}
