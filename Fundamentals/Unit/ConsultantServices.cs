using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Fundamentals.Unit
{
    
    public class ServicesOfConsultant
    {
        public string EnabledServiceId { get; set; }
        public string FeeType { get; set; }
        public double Fee { get; set; }
        public bool IsEnabled { get; set; }
        [BsonIgnore]
        public bool IsCurrent { get; set; }

    }
    public class ConsultantCareer
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId ConsultantId { get; set; }
        public IEnumerable<ServicesOfConsultant> ServicesOffered { get; set; }
        public int TotalCases { get; set; }
        public bool IsActive { get; set; }
        public List<double> Ratings { get;set;}
        [BsonIgnore]
        public double RatingsValue{ get; set; }
        [BsonIgnore]
        public ServicesOfConsultant CurrentService { get { return ServicesOffered?.Where(x => x.IsCurrent == true)?.FirstOrDefault()??new ServicesOfConsultant(); } set { } }
    }


    public class ConsultantVerificationDetails
    {
        public ObjectId ConsultantId { get; set; }
        [Required]
        [Display(Name = "Adhaar")]
        //[RegularExpression(@"^[2-9]{1}[0-9]{3}\\s[0-9]{4}\\s[0-9]{4}$", ErrorMessage = "Please Enter a Valid Adhaar")]
        public string Adhaar { get; set; }
        [Required]
        [Display(Name = "Pancard")]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Please Enter a Valid Pan")]
        public string PanCard { get; set; }
        public string VoterId { get; set; }
        public string DrivingLicense { get; set; }
    }

    public class ConsultantTaxDetails
    { 
        public ObjectId ConsultantId { get; set; }
        public string GST { get; set; }
        public string PanCard { get; set; }
    }
}
