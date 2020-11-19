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
using Users;

namespace PaperWorks
{
    [Authorize(Policy = "RequireConsultantRole")]
    public class MyCaseDetailModel : PageModel
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

        public List<string> AllFileNames { get; set; }
        [BindProperty]
        public string Comment { get; set; }

        [BindProperty]
        public BufferedMultipleFileUploadDb FileUpload { get; set; }
        private readonly string[] _permittedExtensions = { ".txt",".png",".pdf",".jpg",".jpeg" };
        private readonly long _fileSizeLimit = 2097152;

        public MyCaseDetailModel(ICaseManagement caseManagementService , IOrderService orderService,ICaseUpdateService caseUpdateService, UserManager<Clientele> userManager,
            SignInManager<Clientele> signInManager)
        {
            this.caseManagementService = caseManagementService;
            this.orderService = orderService;
            this.caseUpdateService = caseUpdateService;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        public async Task<IActionResult> OnGetAsync(string caseId)
        {
            CurrentCase = caseManagementService.GetCaseById(caseId).Result;
            CurrentOrder = orderService.GetOrderByCaseId(caseId).Result;
            var currentUser = userManager.GetUserAsync(User).Result;
            AllUpdates = caseUpdateService.GetMyUpdates(caseId, currentUser.Email).Result;
            if (AllUpdates == null) AllUpdates = new List<CaseUpdate>();

            Storage store = new Storage();
            AllFileNames = await store.List(caseId);
            return Page();
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
            AllUpdates = caseUpdateService.GetMyUpdates(CaseId, currentUser.Email).Result;
            if (AllUpdates == null) AllUpdates = new List<CaseUpdate>();
            return Partial("_CaseUpdates", AllUpdates);
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
            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.
            Storage store = new Storage();
            await store.Upload(CaseId, FileUpload.FormFiles);

            //CurrentCase = caseManagementService.GetCaseById(CaseId).Result;
            //CurrentOrder = orderService.GetOrderByCaseId(CaseId).Result;
            //AllUpdates = caseUpdateService.GetAllUpdates(CaseId).Result;
            //if (AllUpdates == null) AllUpdates = new List<CaseUpdate>();

            var names = await store.List(CaseId);
            return Partial("_UploadedFileList", names);
        }

        public async Task<IActionResult> OnGetDownloadAsync(string FileName)
        {
            var currentUser = userManager.GetUserAsync(User).Result; 
            CurrentCase = caseManagementService.GetCaseById(CaseId).Result;
            CurrentOrder = orderService.GetOrderByCaseId(CaseId).Result;
            AllUpdates = caseUpdateService.GetMyUpdates(CaseId, currentUser.Email).Result;
            if (AllUpdates == null) AllUpdates = new List<CaseUpdate>();
            
            Storage store = new Storage();
            var blobDto = await store.Download(CaseId, FileName);
            FileStreamResult fileStreamResult = new FileStreamResult(blobDto.Content, blobDto.ContentType);
            fileStreamResult.FileDownloadName = FileName;

            AllFileNames = await store.List(CaseId);

            return fileStreamResult;
        }

        public async Task<IActionResult> OnGetFile(string FileName)
        {
            var currentUser = userManager.GetUserAsync(User).Result;
            CurrentCase = caseManagementService.GetCaseById(CaseId).Result;
            CurrentOrder = orderService.GetOrderByCaseId(CaseId).Result;
            AllUpdates = caseUpdateService.GetMyUpdates(CaseId,currentUser.Email).Result;
            if (AllUpdates == null) AllUpdates = new List<CaseUpdate>();

            Storage store = new Storage();
            var blobDto = await store.Download(CaseId, FileName);
            FileStreamResult fileStreamResult = new FileStreamResult(blobDto.Content, blobDto.ContentType);
            fileStreamResult.FileDownloadName = FileName;

            AllFileNames = await store.List(CaseId);

            return fileStreamResult;
        }


    }
}