using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PaperWorks
{
    public class OrderConfirmationModel : PageModel
    {
        [TempData]
        public string CustomerOrderId { get; set; }
        public void OnGet()
        {

        }
    }
}