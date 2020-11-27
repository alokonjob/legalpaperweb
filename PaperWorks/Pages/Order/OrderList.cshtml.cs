using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaseManagementSpace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using OrderAndPayments;
using User;
using Users;

namespace PaperWorks
{

    public class OrderListModel : PageModel
    {
        private readonly UserManager<Clientele> userManager;
        private readonly SignInManager<Clientele> signInManager;
        private readonly IOrderService orderService;
        private readonly ICaseManagement caseManagement;
        private readonly IPaymentService paymentService;
        private readonly IClienteleServices userServices;

        public class FullOrder
        { 
            public ClienteleOrder Order { get; set; }
            public Case Case { get; set; }
            public ClientelePayment Payment { get; set; }
            public Clientele CaseManager { get; set; }
            public Clientele Consultant { get; set; }

        }

        public List<FullOrder> CompleteOrderInformation { get; set; }
        public List<Clientele> Users { get; set; }
        
        public List<ClienteleOrder> MyOrders { get; set; }
        public List<Case> MyCases { get; set; }
        public List<ClientelePayment> MyPayments { get; set; }
        public OrderListModel(UserManager<Clientele> userManager,
            SignInManager<Clientele> signInManager, IOrderService orderService,ICaseManagement caseManagement,IPaymentService paymentService,IClienteleServices userServices)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.orderService = orderService;
            this.caseManagement = caseManagement;
            this.paymentService = paymentService;
            this.userServices = userServices;
        }
        public async  Task<IActionResult>OnGetAsync(string receipt = "")
        {
            if (userServices.IsSignedIn(User) == false)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity",returnUrl = receipt == "" ? $"/Order/OrderList" : $"/Order/OrderList?receipt={receipt}" });
            }
            var whoIsAsking = await userManager.GetUserAsync(User);
            MyOrders = await orderService.GetOrdersOFUser(whoIsAsking.Id.ToString());
            if (false == string.IsNullOrEmpty(receipt))
            {
                MyOrders = MyOrders.Where(x => string.Compare(receipt, x.Receipt) == 0).ToList();
            }
            MyCases = await caseManagement.GetAllCasesOfUser(whoIsAsking.Email);
            MyPayments = await paymentService.GetPaymentByOrderId(MyOrders.Select(x => x.ClientOrderId).ToList());
            CompleteOrderInformation = new List<FullOrder>();
            ObjectId emptyObjectId = new ObjectId("000000000000000000000000");
            List<ObjectId> userIds = MyCases.Select(x => x.CurrentConsultantId).Distinct().ToList();
            userIds.AddRange(MyCases.Select(x => x.CaseManagerId).Distinct().ToList());
            Users = await userServices.GetUserByIds(userIds);
            foreach (var order in MyOrders)
            {
                FullOrder fullOrder = new FullOrder();
                fullOrder.Order = order;
                fullOrder.Case = MyCases.Where(x => x.CaseId == order.CaseId).FirstOrDefault();
                fullOrder.Payment = MyPayments.Where(x => x.ClienteleOrderId == order.ClientOrderId).FirstOrDefault();
                if (fullOrder.Case.CurrentConsultantId != emptyObjectId)
                {
                    fullOrder.Consultant = Users.Where(x => x.Id == fullOrder.Case.CurrentConsultantId).FirstOrDefault();
                }
                if (fullOrder.Case.CaseManagerId != emptyObjectId)
                {
                    fullOrder.CaseManager = Users.Where(x => x.Id == fullOrder.Case.CaseManagerId).FirstOrDefault();
                }
                CompleteOrderInformation.Add(fullOrder);
            }
            return Page();
        }
    }
}