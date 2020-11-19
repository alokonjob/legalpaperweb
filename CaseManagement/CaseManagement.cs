using Emailer;
using Fundamentals.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using User;
using Users;

namespace CaseManagementSpace
{
    public class CaseManagement : ICaseManagement
    {
        private readonly ICaseRepository caseRepository;
        private readonly IClienteleServices clienteleServices;
        private readonly IClienteleStaffServices staffServices;
        private readonly IEmailer emailSender;
        private readonly ILogger<CaseManagement> logger;

        public CaseManagement(ICaseRepository caseRepository, IClienteleServices clienteleServices, IClienteleStaffServices staffServices, IEmailer emailSender,ILogger<CaseManagement> logger)
        {
            this.caseRepository = caseRepository;
            this.clienteleServices = clienteleServices;
            this.staffServices = staffServices;
            this.emailSender = emailSender;
            this.logger = logger;
        }
        public async Task<ObjectId> GenerateCase(Case customerCase)
        {
            List<Task> tasks = new List<Task>();
            var availableCaseManager = staffServices.FindAvailableCaseManager();
            customerCase.CaseManagerId = availableCaseManager.Id;
            var caseId = caseRepository.AddCase(customerCase);
            tasks.Add(caseId);
            var sendMail = SendEmailToCaseManager(customerCase, availableCaseManager.Email);
            var sendMailToCsutomer = SendEmailToCustomer(customerCase);
            tasks.Add(sendMail);
            tasks.Add(sendMailToCsutomer);
            await Task.WhenAll(tasks.ToArray());
            logger.LogInformation(LogEvents.CreateCase,$"Case.AddCase.Success.ForReceipt.{customerCase.Order.Receipt}");
            return caseId.Result;
        }

        public async Task<List<Case>> GetAllCases()
        {
            return await caseRepository.GetAll();
        }

        public async Task<List<Case>> GetAllCasesOfCaseManager(string userEmail)
        {
            var user = await clienteleServices.GetByEmail(userEmail);
            return await (caseRepository as ICaseManagerCaseRepository).GetCasesOfCaseManager(user.Id.ToString());

        }

        public async Task<List<Case>> GetAllCasesOfConsultant(string userEmail)
        {
            return await (caseRepository as IConsultantCaseRepository).GetCasesOfConsultant(userEmail);
        }

        public async Task<List<Case>> GetAllCasesOfUser(string userEmail)
        {
            return await caseRepository.GetAllCasesOfUser(userEmail);
        }

        public async Task<Case> GetCaseById(string caseId)
        {
            return await caseRepository.GetCaseById(caseId);
        }

        public async Task<Case> GetCaseByReceipt(string receipt)
        {
            return await caseRepository.GetCaseByReceipt(receipt);
        }

        public async Task<Case> UpdateConsultant(Case caseToUpdate)
        {
            return await caseRepository.UpdateConsultant(caseToUpdate);
        }

        public async Task SendEmailToCaseManager(Case newCase, string Email)
        {
            //image is linked, got a publicly embedding link of an image which i uploaded to onedrive
            var url = @"https://bn1304files.storage.live.com/y4mIHFwD52DVr94Jn_fkVhvxLvKqINovh-_VXYz-qVvDpzFxF8qtaBjWbEuOqWMl67ZPYgDFU78763JFpzjd2-A-TlaRocZsPLaauT1N7k-US-3rBciIupSV0hu9pl6BDU3bV_aXHGVUab0ViPNIKcc4NoRSIsn-2oSnXv6Sah8NQNjkFvT8o8QUkakhCBJpTzs?width=1024&height=427&cropmode=none";
            var template = @$"
<img alt=""My Image"" src=""{url}"" />
             <table>
<tr>
                    <td>
                     Customer Name 
                    </td>
                    <td>
                    ##Name
                    </td>
                </tr>
                <tr>
                    <td>
                     Customer Phone 
                    </td>
                    <td>
                    ##Phone
                    </td>
                </tr>
                    <tr>
                    <td>
                     Customer Email 
                    </td>
                    <td>
                    ##Email
                    </td>
                    </tr>
                    <tr>
                    <td>
                     Service
                    </td>
                    <td>
                    ##Service
                    </td>
                    </tr>
                    <tr>
                    <td>
                     City
                    </td>
                    <td>
                    ##City
                    </td>
                    </tr>
</table>
";
            StringBuilder sb = new StringBuilder();
            sb.Append(template.Replace("##Name", newCase.Order.CustomerName).Replace("##Phone", newCase.Order.CustomerPhone).Replace("##Email", newCase.Order.CustomerEmail).Replace("##Service", newCase.Order.ServiceName).Replace("##City", newCase.Order.City)
            );
            await emailSender.SendEmailAsync(Email, $"[New Case] {newCase.Order.ServiceName} in {newCase.Order.City} - Receipt {newCase.Order.Receipt}", sb.ToString());
        }

        public async Task SendEmailToCustomer(Case newCase)
        {
            //image is linked, got a publicly embedding link of an image which i uploaded to onedrive
            var url = @"https://bn1304files.storage.live.com/y4mIHFwD52DVr94Jn_fkVhvxLvKqINovh-_VXYz-qVvDpzFxF8qtaBjWbEuOqWMl67ZPYgDFU78763JFpzjd2-A-TlaRocZsPLaauT1N7k-US-3rBciIupSV0hu9pl6BDU3bV_aXHGVUab0ViPNIKcc4NoRSIsn-2oSnXv6Sah8NQNjkFvT8o8QUkakhCBJpTzs?width=660&height=275&cropmode=none";
            var template = @$"
<img alt=""My Image"" src=""{url}"" />
             <table>
<tr>
                    <td>
                     <strong>Customer Name </strong>
                    </td>
                    <td>
                    ##Name
                    </td>
                </tr>
                <tr>
                    <td>
                    <strong> Customer Phone </strong>
                    </td>
                    <td>
                    ##Phone
                    </td>
                </tr>
                    <tr>
                    <td>
                     </strong>Customer Email </strong>
                    </td>
                    <td>
                    ##Email
                    </td>
                    </tr>
                    <tr>
                    <td>
                     </strong>Service</strong>
                    </td>
                    <td>
                    ##Service
                    </td>
                    </tr>
                    <tr>
                    <td>
                     </strong>City</strong>
                    </td>
                    <td>
                    ##City
                    </td>
                    </tr>
</table>
<br>


<br>
<p>
We have your Order. You will shortly hear from our Case Manager with updates.<br>
Requesting you to kindly keep documents ready.<br>
If you have any doubts regarding the documents, share the same with Case Manager.<br>
He will be happy to share with you the details of the proces.<br>
</p>
";
            StringBuilder sb = new StringBuilder();
            sb.Append(template.Replace("##Name", newCase.Order.CustomerName).Replace("##Phone", newCase.Order.CustomerPhone).Replace("##Email", newCase.Order.CustomerEmail).Replace("##Service", newCase.Order.ServiceName).Replace("##City", newCase.Order.City)
            );
            await emailSender.SendEmailAsync(newCase.Order.CustomerEmail, $"Thank you ! Your Order is Confirmed - Receipt {newCase.Order.Receipt}", sb.ToString());
        }
    }
}
