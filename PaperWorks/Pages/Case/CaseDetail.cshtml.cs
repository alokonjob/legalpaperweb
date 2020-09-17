using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaseManagement;
using CaseManagementSpace;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using OrderAndPayments;
using Users;

namespace PaperWorks
{
    public class CaseDetailModel : PageModel
    {
        private readonly ICaseManagement caseManagementService;
        private readonly IOrderService orderService;
        private readonly ICaseUpdateService caseUpdateService;
        private readonly UserManager<Clientele> userManager;
        private readonly SignInManager<Clientele> signInManager;

        [BindProperty(SupportsGet = true)]
        public string CaseId { get; set; }
        [BindProperty]
        public Case CurrentCase { get; set; }
        [BindProperty]
        public ClienteleOrder CurrentOrder { get; set; }
        public List<CaseUpdate> AllUpdates { get; set; }
        [BindProperty]
        public string Comment { get; set; }
        public CaseDetailModel(ICaseManagement caseManagementService , IOrderService orderService,ICaseUpdateService caseUpdateService, UserManager<Clientele> userManager,
            SignInManager<Clientele> signInManager)
        {
            this.caseManagementService = caseManagementService;
            this.orderService = orderService;
            this.caseUpdateService = caseUpdateService;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        public void OnGet(string caseId)
        {
            CurrentCase = caseManagementService.GetCaseById(caseId).Result;
            CurrentOrder = orderService.GetOrderByCaseId(caseId).Result;
            AllUpdates = caseUpdateService.GetAllUpdates(caseId).Result;
            if (AllUpdates == null) AllUpdates = new List<CaseUpdate>();
        }


        public PartialViewResult OnPostAddUpdate()
        {
            CurrentCase = caseManagementService.GetCaseById(CaseId).Result;
            CurrentOrder = orderService.GetOrderByCaseId(CaseId).Result;
            
            CaseUpdate update = new CaseUpdate();
            var currentUser = userManager.GetUserAsync(User).Result;
            update.UpdatedBy = new AbridgedUser() { Email = currentUser.Email, FullName = currentUser.FullName, PhoneNumber = currentUser.PhoneNumber };
            update.Comment = Comment;
            update.CaseId = ObjectId.Parse(CaseId);
            update.UpdatedDate = DateTime.UtcNow;
            var newUpdate = caseUpdateService.AddUpdate(update).Result;
            AllUpdates = caseUpdateService.GetAllUpdates(CaseId).Result;
            if (AllUpdates == null) AllUpdates = new List<CaseUpdate>();
            return Partial("_CaseUpdates", AllUpdates);
        }
    }
}