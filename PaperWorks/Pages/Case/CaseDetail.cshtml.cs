using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CaseManagement;
using CaseManagementSpace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using OrderAndPayments;
using SampleApp.Utilities;

using Store;
using User;
using Users;

namespace PaperWorks
{
    [Authorize(Policy = "UpdateCasesPolicy")]
    public class CaseDetailModel : PageModel
    {
        private readonly ICaseManagement caseManagementService;
        private readonly IOrderService orderService;
        private readonly ICaseUpdateService caseUpdateService;
        private readonly IClienteleServices clienteleServices;
        private readonly ICasePaymentReleaseService casePaymentService;
        private readonly INudgeService nudgeService;

        [BindProperty(SupportsGet = true)]
        public string Receipt { get; set; }
        [BindProperty]
        public Case CurrentCase { get; set; }
        [BindProperty]
        public ClienteleOrder CurrentOrder { get; set; }
        public List<CaseUpdate> AllUpdates { get; set; }
        public Clientele CaseManager { get; set; }

        public List<string> AllFileNames { get; set; }

        public PayToConsultant ConsultantPay { get; set; }
        [BindProperty]
        public UpdateInput UserUpdate { get; set; }
        public class UpdateInput
        {
            public string Comment { get; set; }
            public string ConsultantEmail { get; set; }
        }

        public class NudgeInput
        {
            [DataType(DataType.Currency, ErrorMessage = "Please Enter Only Numbers")]
            public string NudgeAmount { get; set; }
            public NudgeType TypeOfNudge { get; set; }
        }

        [BindProperty(SupportsGet =true)]
        public NudgeInput InputForNudge { get; set; }

        [BindProperty]
        public BufferedMultipleFileUploadDb FileUpload { get; set; }
        private readonly string[] _permittedExtensions = { ".txt", ".png", ".pdf", ".jpg", ".jpeg" };
        private readonly long _fileSizeLimit = 2097152;

        public CaseDetailModel(ICaseManagement caseManagementService, IOrderService orderService, ICaseUpdateService caseUpdateService, IClienteleServices clienteleServices,
             ICasePaymentReleaseService casePaymentService, INudgeService nudgeService)
        {
            this.caseManagementService = caseManagementService;
            this.orderService = orderService;
            this.caseUpdateService = caseUpdateService;
            this.clienteleServices = clienteleServices;

            this.casePaymentService = casePaymentService;
            this.nudgeService = nudgeService;
        }
        public async Task<IActionResult> OnGetAsync(string rct)
        {
            var tasks = new List<Task>();
            Task<PayToConsultant> PayInfoTask;
            Task<ClienteleOrder> CurrentOrderTask;
            //Task<Case> GetcurrentCaseTask;
            Task<List<CaseUpdate>> GetAllUpdatesTask;
            Task<List<string>> GetAllFilesTask;
            Task<NudgeType> FindActiveNudget;

            Receipt = rct;

            var customGetcurrentCaseTask = await caseManagementService.GetCaseByReceipt(rct);
            var cM = await clienteleServices.GetUserByIds(new List<ObjectId>() { customGetcurrentCaseTask.CaseManagerId });
            CaseManager = cM[0];
            var caseId = customGetcurrentCaseTask.CaseId.ToString();



            CurrentOrderTask = orderService.GetOrderByReceipt(rct);
            tasks.Add(CurrentOrderTask);


            //tasks.Add(GetcurrentCaseTask);

            GetAllUpdatesTask = caseUpdateService.GetAllUpdates(caseId,User.IsFounder() ? true : false);
            tasks.Add(GetAllUpdatesTask);


            Storage store = new Storage();
            GetAllFilesTask = store.List(caseId);
            tasks.Add(GetAllFilesTask);

            CurrentCase = customGetcurrentCaseTask;// caseManagementService.GetCaseById(caseId).Result;

            PayInfoTask = casePaymentService.GetPaymentsForCase(caseId, CurrentCase.CurrentConsultantId.ToString());
            //GetcurrentCaseTask.ContinueWith((i) =>
            //{
            //    //if (User.IsFinanceUser())
            //    {
            //        ConsultantPay = casePaymentService.GetPaymentsForCase(caseId, CurrentCase.CurrentConsultantId.ToString()).Result;
            //    }
            //}, TaskContinuationOptions.OnlyOnRanToCompletion);
            tasks.Add(PayInfoTask);

            FindActiveNudget = nudgeService.IsNudgeOn(Receipt);
            tasks.Add(FindActiveNudget);

            await Task.WhenAll(tasks.ToArray());


            CurrentOrder = CurrentOrderTask.Result;//  orderService.GetOrderByCaseId(caseId).Result;
            AllUpdates = GetAllUpdatesTask.Result;
            if (AllUpdates == null) AllUpdates = new List<CaseUpdate>();

            InputForNudge = new NudgeInput();
            InputForNudge.TypeOfNudge = FindActiveNudget.Result;

            AllFileNames = await store.List(caseId);
            ConsultantPay = PayInfoTask.Result;


            return Page();
        }


        public async Task<PartialViewResult> OnPostAddUpdate()
        {
            CurrentCase = await caseManagementService.GetCaseByReceipt(Receipt);

            CaseUpdate update = new CaseUpdate();
            var currentUser = clienteleServices.GetUserAsync(User).Result;
            
            update.UpdatedBy = new AbridgedUser() { Email = currentUser.Email, FullName = currentUser.FullName, PhoneNumber = currentUser.PhoneNumber };
            update.Comment = UserUpdate.Comment;
            update.ShareWithConsultantEmail = UserUpdate.ConsultantEmail;
            update.CaseId = CurrentCase.CaseId;
            update.UpdatedDate = DateTime.UtcNow;

            await  caseUpdateService.AddUpdate(update);

            AllUpdates = await caseUpdateService.GetAllUpdates(CurrentCase.CaseId.ToString(), User.IsFounder() ? true : false);
            ConsultantPay = await casePaymentService.GetPaymentsForCase(CurrentCase.CaseId.ToString(), CurrentCase.CurrentConsultantId.ToString());
            if (AllUpdates == null) AllUpdates = new List<CaseUpdate>();

            return Partial("_CaseUpdates", AllUpdates);
        }

        public async Task<IActionResult> OnPostDeleteUpdate()
        {
            var caseUpdate = await caseUpdateService.RemoveUpdate(Request.Form["UpdateId"]);

            AllUpdates = await caseUpdateService.GetAllUpdates(caseUpdate.CaseId.ToString(), User.IsFounder() ? true : false);
            if (AllUpdates == null) AllUpdates = new List<CaseUpdate>();

            return RedirectToPage("/Case/CaseListing");
        }

        public async Task<PartialViewResult> OnPostUploadAsync()
        {
            foreach (var formFile in FileUpload.FormFiles)
            {
                var formFileContent =
                    await FileHelpers.ProcessFormFile<BufferedMultipleFileUploadDb>(
                        formFile, ModelState, _permittedExtensions,
                        _fileSizeLimit);

                // **WARNING!**
                // In the following example, the file is saved without
                // scanning the file's contents. In most production
                // scenarios, an anti-virus/anti-malware scanner API
                // is used on the file before making the file available
                // for download or for use by other systems. 
                // For more information, see the topic that accompanies 
                // this sample.
            }
            var caseOrder = await caseManagementService.GetCaseByReceipt(Receipt);
            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.
            Storage store = new Storage();
            await store.Upload(caseOrder.CaseId.ToString(), FileUpload.FormFiles);

            //CurrentCase = caseManagementService.GetCaseById(CaseId).Result;
            //CurrentOrder = orderService.GetOrderByCaseId(CaseId).Result;
            //AllUpdates = caseUpdateService.GetAllUpdates(CaseId).Result;
            //if (AllUpdates == null) AllUpdates = new List<CaseUpdate>();

            var names = await store.List(caseOrder.CaseId.ToString());
            return Partial("_UploadedFileList", names);
        }

        public async Task<IActionResult> OnGetDownloadAsync(string FileName)
        {
            CurrentCase = caseManagementService.GetCaseByReceipt(Receipt).Result;
            CurrentOrder = orderService.GetOrderByReceipt(Receipt).Result;
            AllUpdates = caseUpdateService.GetAllUpdates(CurrentCase.CaseId.ToString()).Result;
            if (AllUpdates == null) AllUpdates = new List<CaseUpdate>();

            Storage store = new Storage();
            var blobDto = await store.Download(CurrentCase.CaseId.ToString(), FileName);
            FileStreamResult fileStreamResult = new FileStreamResult(blobDto.Content, blobDto.ContentType);
            fileStreamResult.FileDownloadName = FileName;

            AllFileNames = await store.List(CurrentCase.CaseId.ToString());

            return fileStreamResult;
        }

        public async Task<IActionResult> OnGetFile(string FileName)
        {
            CurrentCase = caseManagementService.GetCaseByReceipt(Receipt).Result;
            CurrentOrder = orderService.GetOrderByReceipt(Receipt).Result;
            AllUpdates = caseUpdateService.GetAllUpdates(CurrentCase.CaseId.ToString()).Result;
            if (AllUpdates == null) AllUpdates = new List<CaseUpdate>();

            Storage store = new Storage();
            var blobDto = await store.Download(CurrentCase.CaseId.ToString(), FileName);
            FileStreamResult fileStreamResult = new FileStreamResult(blobDto.Content, blobDto.ContentType);
            fileStreamResult.FileDownloadName = FileName;

            AllFileNames = await store.List(CurrentCase.CaseId.ToString());

            return fileStreamResult;
        }


        public async Task<IActionResult> OnPostNudge()
        {
            NudgeType isNudgeOn = await nudgeService.IsNudgeOn(Receipt);
            if (isNudgeOn == NudgeType.None)
            {
                await caseManagementService.ChangeStatus(Receipt, InputForNudge.TypeOfNudge == NudgeType.RequestCaseClosure ?  CaseStatus.CaseManagerClosed : CaseStatus.InProgress);
                await nudgeService.MakeANudge(User, Receipt, InputForNudge.TypeOfNudge, InputForNudge.NudgeAmount);
            }
            return RedirectToPage("/Case/CaseListing");
        }


    }

    public class BufferedMultipleFileUploadDb
    {
        [Required]
        [Display(Name = "File")]
        public List<IFormFile> FormFiles { get; set; }

        [Display(Name = "Note")]
        [StringLength(50, MinimumLength = 0)]
        public string Note { get; set; }
    }
}