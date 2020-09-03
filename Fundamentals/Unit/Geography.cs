using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fundamentals.Unit
{
    public class Geography
    {
        [BsonId(IdGenerator =typeof(GuidGenerator))]
        public Object GeoId  { get; set; }
        [Required] 
        public string City { get; set; }
        public string State { get; set; }
        public bool IsActive { get; set; }

    }
}
