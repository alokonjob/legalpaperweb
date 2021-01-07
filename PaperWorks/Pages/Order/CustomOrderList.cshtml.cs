using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrderAndPayments;
using User;

namespace PaperWorks
{
    public class CustomOrderListModel : PageModel
    {
        private readonly IOrderService orderService;

        public CustomOrderListModel(IOrderService orderService)
        {
            this.orderService = orderService;
        }
        public List<ClienteleOrder> OrderList{ get; set; }
        public async Task<IActionResult> OnGet()
        {
            if (User.IsFinanceUser() || User.IsFounder())
            {
                OrderList = await orderService.GetCustomOrders();
            }
            else if (User.IsCaseManager())
            {
                OrderList = await orderService.GetCustomOrders(User.Identity.Name);
            }
           return Page();
        }
    }
}