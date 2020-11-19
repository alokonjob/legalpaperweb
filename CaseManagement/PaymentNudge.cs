using CaseManagementSpace;
using Fundamentals.DbContext;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using User;

namespace CaseManagement
{
    public class PaymentNudge
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))] 
        public ObjectId Id { get; set; }
        public ObjectId CaseId { get; set; }
        public List<NudgeInfo> Nudges { get; set; }
    }

    public class NudgeInfo
    {
        public double Amount { get; set; }
        public DateTime NudgeOnDate { get; set; }
        public DateTime NudgeCompleteDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsOn { get; set; }
    }

    public interface IPaymentNudgeService
    {
        void EndANudge(string EndedBy, string receipt);
        Task<bool> IsNudgeOn(string receipt);
        Task<PaymentNudge> MakeANudge(ClaimsPrincipal User, string receipt, string amount);
        Task<NudgeInfo> GetNudge(string receipt);
    }

    public class PaymentNudgeService : IPaymentNudgeService
    {
        private readonly ICaseManagement caseManagementService;
        private readonly IClienteleServices clientServices;
        private readonly IPaymentNudgeRepository nudgeRepo;

        public PaymentNudgeService(ICaseManagement caseManagementService, IClienteleServices clientServices, IPaymentNudgeRepository nudgeRepo)
        {
            this.caseManagementService = caseManagementService;
            this.clientServices = clientServices;
            this.nudgeRepo = nudgeRepo;
        }

        public async Task<PaymentNudge> MakeANudge(ClaimsPrincipal User, string receipt, string amount)
        {
            var clientCase = caseManagementService.GetCaseByReceipt(receipt);
            var user = clientServices.GetUserAsync(User);
            NudgeInfo info = new NudgeInfo();
            info.Amount = Convert.ToDouble(amount);
            info.IsOn = true;
            info.CreatedBy = user.Result.Email;
            info.ModifiedBy = user.Result.Email;
            info.NudgeOnDate = DateTime.UtcNow;
            PaymentNudge pNudget = new PaymentNudge();
            pNudget.CaseId = clientCase.Result.CaseId;
            pNudget.Nudges = new List<NudgeInfo>() { info };
            return await nudgeRepo.AddNudge(pNudget);

        }

        public void EndANudge(string EndedBy , string receipt)
        {
            var clientCase = caseManagementService.GetCaseByReceipt(receipt);
            nudgeRepo.EndNudge(EndedBy, clientCase.Result.CaseId.ToString());

        }

        public async Task<NudgeInfo> GetNudge(string receipt)
        {
            var clientCase = await caseManagementService.GetCaseByReceipt(receipt);
            var nudges = await nudgeRepo.GetNudges(clientCase.CaseId.ToString());
            return nudges!=null ? nudges.Nudges.Where(x => x.IsOn == true).FirstOrDefault() : null;
        }

        public async Task<bool> IsNudgeOn(string receipt)
        {
            var clientCase = await caseManagementService.GetCaseByReceipt(receipt);
            var nudge = await nudgeRepo.GetNudges(clientCase.CaseId.ToString());
            return nudge != null ? nudge.Nudges.Select(x => x.IsOn == true).FirstOrDefault() : false;
        }
    }

    public interface IPaymentNudgeRepository
    {
        Task<PaymentNudge> AddNudge(PaymentNudge nudge);
        Task<PaymentNudge> GetNudges(string caseId);
        Task EndNudge(string endedBy, string caseId);
    }

    public class PaymentNudgeRepository : IPaymentNudgeRepository
    {
        IMongoClient client = null;
        private readonly IMongoCollection<PaymentNudge> _nudgeCollection;
        public PaymentNudgeRepository(IMongoDbContext mongoContext)
        {
            client = mongoContext.GetMongoClient();
            _nudgeCollection = mongoContext.GetCollection<PaymentNudge>(client, "PaymentNudge");
        }

        public async Task<PaymentNudge> AddNudge(PaymentNudge nudge)
        {
            var filter = Builders<PaymentNudge>.Filter.Eq(x => x.CaseId, nudge.CaseId);
            var existingNudge = await _nudgeCollection.FindAsync(filter);
            if (existingNudge.FirstOrDefault() == null)
            {
                await _nudgeCollection.InsertOneAsync(nudge);
            }
            else
            {
                await _nudgeCollection.UpdateOneAsync(filter, Builders<PaymentNudge>.Update
                    .Push<NudgeInfo>(e => e.Nudges, nudge.Nudges[0])

                    );
            }

            return nudge;
        }

        public async Task<PaymentNudge> GetNudges(string caseId)
        {
            ObjectId caseObjectId = ObjectId.Parse(caseId);
            var filter = Builders<PaymentNudge>.Filter.Eq(x => x.CaseId, caseObjectId);
            var existingNudge =  await _nudgeCollection.FindAsync<PaymentNudge>(filter);
            return existingNudge.FirstOrDefault();
        }


        public async Task EndNudge(string endedBy,string caseId)
        {
            ObjectId caseObjectId = ObjectId.Parse(caseId);
            var filter = Builders<PaymentNudge>.Filter.Where(x => x.CaseId ==  caseObjectId && x.Nudges.Any(y=>y.IsOn == true));
            var update = Builders<PaymentNudge>.Update.Set(x => x.Nudges[-1].IsOn, false).Set(x=>x.Nudges[-1].ModifiedBy, endedBy);
            var result = await _nudgeCollection.UpdateOneAsync(filter, update);
        }
    }

}
