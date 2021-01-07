using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CaseManagement;
using CaseManagementSpace;
using Consultant;
using Emailer;
using Fundamentals.Extensions;
using Fundamentals.Unit;
using Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrderAndPayments;
using shortid;
using shortid.Configuration;
using User;
using Users;

namespace PaperWorks
{
    public class AssignCaseModel : PageModel
    {
        private readonly ICaseManagement caseManagement;
        private readonly IConsultantCareerManagement consultantManagement;
        private readonly IClienteleServices clientServices;
        private readonly ICasePaymentReleaseService casePaymentReleaseService;
        private readonly ILogger<AssignCaseModel> logger;
        private readonly IStringLocalizer<CaseDetailModel> localizer;
        private readonly IEmailer emailSender;
        private readonly IMessaging smsService;
        public class ConsultantFinal
        {
            public double FinalizedFee { get; set; }
            public string CEmail { get; set; }
            public bool Selected{ get; set; }
        }

        [BindProperty(SupportsGet = true)]
        public List<ConsultantFinal> FinalizedDetails { get; set; }

        [Required]
        [BindProperty(SupportsGet =true)]
        public string SeletedEmail{ get; set; }

        [Required(ErrorMessage = "Required Field")]
        [BindProperty(SupportsGet = true)]
        public string FinalizedCost { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Receipt { get; set; }
        public List<UserUIInfo> FullConsultantDetails { get; set; }
        

        [BindProperty(SupportsGet = true)]
        public string ReasonOfAssignment { get; set; }
        public AssignCaseModel(ICaseManagement caseManagement, IConsultantCareerManagement consultantManagement, IClienteleServices clientServices,
            ICasePaymentReleaseService casePaymentReleaseService , ILogger<AssignCaseModel> logger, IStringLocalizer<CaseDetailModel> localizer, IEmailer emailSender, IMessaging smsService)
        {
            this.caseManagement = caseManagement;
            this.consultantManagement = consultantManagement;
            this.clientServices = clientServices;
            this.casePaymentReleaseService = casePaymentReleaseService;
            this.logger = logger;
            this.localizer = localizer;
            this.emailSender = emailSender;
            this.smsService = smsService;
        }
        public async Task<IActionResult> OnGetAsync(string rct)
        {
            var cases = await caseManagement.GetCaseByReceipt(rct);
            Receipt  = rct;
            var consultants = await consultantManagement.GetConsultantForEnabledService(cases.Order.ServiceName, cases.Order.City);
            var userIds = consultants.Select(x => x.ConsultantId).ToList();
            var consulTatDetails = await clientServices.GetUserByIds(userIds);
            FullConsultantDetails = consultants.Select(x=> new UserUIInfo() { ConsultantDetails = x , UserDetails = consulTatDetails.Where(y=>y.Id ==  x.ConsultantId).FirstOrDefault() })
                .ToList()
                .OrderBy(x=>  x.ConsultantDetails.CurrentService.Fee )
                .ThenByDescending(x => x.ConsultantDetails.RatingsValue).ToList();
            FinalizedDetails = FullConsultantDetails.Select(x => new ConsultantFinal() { FinalizedFee = x.ConsultantDetails.CurrentService.Fee,CEmail = x.UserDetails.Email }).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (FinalizedCost == null)
                { 
                    
                }
                var userDetails = await clientServices.GetByEmail(SeletedEmail);
                var existingCaseDetail = await caseManagement.GetCaseByReceipt(Receipt);
                Case caseToUpdate = new Case();
                caseToUpdate.Order = new AbridgedOrder();
                caseToUpdate.Order.ConsultantEmail = SeletedEmail;
                caseToUpdate.Order.ConsultantPhone = userDetails.PhoneNumber;
                caseToUpdate.CurrentConsultantId = userDetails.Id;
                caseToUpdate.CaseId = existingCaseDetail.CaseId;
                if (existingCaseDetail.CurrentConsultantId.ToString() != "000000000000000000000000")
                {
                    caseToUpdate.PreviousConsultantId = new List<MongoDB.Bson.ObjectId>() { existingCaseDetail.CurrentConsultantId };
                }
                else
                {
                    caseToUpdate.PreviousConsultantId = new List<MongoDB.Bson.ObjectId>();
                }
                var casepaymentInfo = casePaymentReleaseService.SetFinalizedCost(caseToUpdate.CaseId.ToString(), caseToUpdate.CurrentConsultantId.ToString(), Convert.ToDouble(FinalizedCost));

                var options = new GenerationOptions
                {
                    UseNumbers = true,
                    UseSpecialCharacters = true,
                    Length = 15,
                };
                

                var caseConfirmationCode = ShortId.Generate(options).ToUpper();
                caseToUpdate.CaseConfirmationCode = caseConfirmationCode;
                caseToUpdate.CaseConfirmedBy = string.Empty;
                caseToUpdate.CurrentStatus = CaseStatus.PendingConsultantConfirmation;

                var user = caseManagement.UpdateConsultant(caseToUpdate);

                var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(caseConfirmationCode));
                var encodedRct = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(Receipt));
                var callbackUrl = Url.Page(
                    "/Case/ConfirmCase",
                    pageHandler: null,
                    values: new {  userId = SeletedEmail, code = encodedCode, rct = encodedRct, returnUrl = Url.Content("~/") },
                    protocol: Request.Scheme);
                Dictionary<string, string> itemDictionary = new Dictionary<string, string>();
                itemDictionary.Add("##SERVICE", localizer[existingCaseDetail.Order.ServiceDisplayName]);
                itemDictionary.Add("##CITY", existingCaseDetail.Order.City.ToUpper());
                itemDictionary.Add("##FEE", FinalizedCost);
                itemDictionary.Add("##CUSTOMERNAME", existingCaseDetail.Order.CustomerName);
                itemDictionary.Add("##URL", HtmlEncoder.Default.Encode(callbackUrl));

                var emailSendingTask =  emailSender.SendCaseConfirmationEmail(itemDictionary, caseToUpdate.Order.ConsultantEmail);
                await Task.WhenAll(new List<Task>() { casepaymentInfo, user, emailSendingTask });
                if (user != null)
                {
                    return RedirectToPage("./CaseDetail", new { rct = Receipt });
                }
                return Page();
            }
            catch (Exception error)
            {
                logger.LogCritical(error, error.Message);
                throw;
            }
        }

        public async Task SendCommunicationToConsultant(Case customerCase, Dictionary<string, string> TemplateFillers)
        {
            //image is linked, got a publicly embedding link of an image which i uploaded to onedrive

            await emailSender.SendNewOrderMail(TemplateFillers, customerCase.Order.CustomerEmail);
            var text = $"Unpaperwork Acceptance Needed {TemplateFillers["##SERVICE"]}  for {TemplateFillers["##SERVICE"]} in city {TemplateFillers["##CITY"]}. Kindly check email and confirm. If not accepted in 2 hours , case shall be reassigned.";
            smsService.SendSMS(customerCase.Order.ConsultantPhone, text);
        }


        public class UserUIInfo
        { 
            public ConsultantCareer ConsultantDetails { get; set; }
            public Clientele UserDetails { get; set; }
            public bool Selected { get; set; }
        }
    }
}