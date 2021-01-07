using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace User
{
    public class ContactInput
    {

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^([1-9][0-9]{9})$", ErrorMessage = "Invalid Phone Number.")]
        [Display(Name = "Phone/Mobile number")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Subject is required")]
        [MaxLength(500, ErrorMessage = "Not More than 500 characters allowed")]
        public string Subject { get; set; }
        [Required]
        [MaxLength(3000, ErrorMessage = "Not More than 3000 characters allowed")]
        public string Message { get; set; }

    }
}
