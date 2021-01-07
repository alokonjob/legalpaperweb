using CaseManagementSpace;
using Fundamentals.DbContext;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using User;

namespace CaseManagement
{
    public interface INudge
    {
        public ObjectId Id { get; set; }
        public ObjectId CaseId { get; set; }
        public List<NudgeInfo> Nudges { get; set; }
    }

    public enum NudgeType
    {
        [Description("None")]
        None = 0,
        [Description("Payment")]
        ConsultantPaymentNudge = 10 ,
        [Description("Closure")]
        RequestCaseClosure = 100
    }

    public abstract class NudgeBase : INudge
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId Id { get; set; }
        public ObjectId CaseId { get; set; }
        public List<NudgeInfo> Nudges { get; set; }
        
    }

    public class Nudge : NudgeBase
    {
    }


    public class NudgeInfo
    {
        public DateTime NudgeOnDate { get; set; }
        public DateTime NudgeCompleteDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsOn { get; set; }
        public NudgeType TypeOfNudge { get; set; }
        public object Amount { get; set; }
    }

    public interface INudgeService
    {
        void EndANudge(string EndedBy, string receipt);
        Task<NudgeType> IsNudgeOn(string receipt);
        Task<Nudge> MakeANudge(ClaimsPrincipal User, string receipt,NudgeType typeOfNudge, object amount);
        Task<NudgeInfo> GetNudge(string receipt);
    }

    public class NudgeService : INudgeService
    {
        private readonly ICaseManagement caseManagementService;
        private readonly IClienteleServices clientServices;
        private readonly INudgeRepository nudgeRepo;

        public NudgeService(ICaseManagement caseManagementService, IClienteleServices clientServices, INudgeRepository nudgeRepo)
        {
            this.caseManagementService = caseManagementService;
            this.clientServices = clientServices;
            this.nudgeRepo = nudgeRepo;
        }

        public async Task<Nudge> MakeANudge(ClaimsPrincipal User, string receipt, NudgeType typeOfNudge, object amount)
        {
            var clientCase = caseManagementService.GetCaseByReceipt(receipt);
            var user = clientServices.GetUserAsync(User);
            NudgeInfo info = new NudgeInfo();
            info.Amount = amount;
            info.IsOn = true;
            info.CreatedBy = user.Result.Email;
            info.ModifiedBy = user.Result.Email;
            info.NudgeOnDate = DateTime.UtcNow;
            info.TypeOfNudge = typeOfNudge;
            Nudge pNudget = new Nudge();
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

        public async Task<NudgeType> IsNudgeOn(string receipt)
        {
            var clientCase = await caseManagementService.GetCaseByReceipt(receipt);
            var nudge = await nudgeRepo.GetNudges(clientCase.CaseId.ToString());
            if (nudge != null)
            {
                var nudgeInfo = nudge.Nudges.Where(x => x.IsOn == true).FirstOrDefault();
                if(nudgeInfo != null)
                {
                    return nudgeInfo.TypeOfNudge;
                }
            }
            return NudgeType.None;
        }
    }

    public interface INudgeRepository
    {
        Task<Nudge> AddNudge(Nudge nudge);
        Task<Nudge> GetNudges(string caseId);
        Task EndNudge(string endedBy, string caseId);
    }

    public class NudgeRepository : INudgeRepository
    {
        IMongoClient client = null;
        private readonly IMongoCollection<Nudge> _nudgeCollection;
        public NudgeRepository(IMongoDbContext mongoContext)
        {
            client = mongoContext.GetMongoClient();
            _nudgeCollection = mongoContext.GetCollection<Nudge>(client, "PaymentNudge");
        }

        public async Task<Nudge> AddNudge(Nudge nudge)
        {
            var filter = Builders<Nudge>.Filter.Eq(x => x.CaseId, nudge.CaseId);
            var existingNudge = await _nudgeCollection.FindAsync(filter);
            if (existingNudge.FirstOrDefault() == null)
            {
                await _nudgeCollection.InsertOneAsync(nudge);
            }
            else
            {
                await _nudgeCollection.UpdateOneAsync(filter, Builders<Nudge>.Update
                    .Push<NudgeInfo>(e => e.Nudges, nudge.Nudges[0])

                    );
            }

            return nudge;
        }

        public async Task<Nudge> GetNudges(string caseId)
        {
            ObjectId caseObjectId = ObjectId.Parse(caseId);
            var filter = Builders<Nudge>.Filter.Eq(x => x.CaseId, caseObjectId);
            var existingNudge =  await _nudgeCollection.FindAsync<Nudge>(filter);
            return existingNudge.FirstOrDefault();
        }


        public async Task EndNudge(string endedBy,string caseId)
        {
            ObjectId caseObjectId = ObjectId.Parse(caseId);
            var filter = Builders<Nudge>.Filter.Where(x => x.CaseId ==  caseObjectId && x.Nudges.Any(y=>y.IsOn == true));
            var update = Builders<Nudge>.Update.Set(x => x.Nudges[-1].IsOn, false).Set(x=>x.Nudges[-1].ModifiedBy, endedBy);
            var result = await _nudgeCollection.UpdateOneAsync(filter, update);
        }
    }

}
