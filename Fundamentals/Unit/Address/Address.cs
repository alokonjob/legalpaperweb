using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FundamentalAddress
{
    public class UserAddress
    {
        [Required(ErrorMessage = "House Number is required")]
        [Display(Name = "Flat, House/Shop no., Apartment")]
        public string HouseNo { get; set; }
        [Required(ErrorMessage = "Locality is required")]
        [Display(Name = "Area, Colony, Street, Sector, Village")]
        public string Locality { get; set; }
        [Display(Name = "Landmark")]
        public string LandMark { get; set; }
        [Required(ErrorMessage = "Town/City is required")]
        [Display(Name = "Town/City")]
        public string City { get; set; }
        [Required(ErrorMessage = "State/Union Territory is required")]
        [Display(Name = "State/Union Territory")]
        public string State { get; set; }
        [Required(ErrorMessage = "Country/Region is required")]
        [Display(Name = "Country/Region")]
        public string Country { get; set; }
        [Required]
        [Display(Name = "Pin Code 6 digits [0-9]")] 
        [RegularExpression("^[1-9][0-9]{5}$",ErrorMessage ="Only six numbers are allowed")]
        public string Pin { get; set; }
        [Display(Name = "Home or Office")]
        public string AddressType {get;set;}
    }
}
