using Fundamentals.DbContext;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using OrderAndPayments;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Audit
{
    public class OrderAudit
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))] 
        public ObjectId AuditId { get; set; }
        public string Email { get; set; }
        public string OrderReceipt { get; set; }
        public List<string> History { get; set; }
        public bool IsComplete { get; set; }
        public DateTime DateOfOrder { get; set; }
    }

    public class OrderAuditService : IOrderAuditService
    {
        private readonly IOrderAuditRepository auditRepo;

        public OrderAuditService(IOrderAuditRepository auditRepo)
        {
            this.auditRepo = auditRepo;
        }

        public async Task<ObjectId> AddAudit(OrderAudit orderAudit)
        {
            return await auditRepo.AddAudit(orderAudit);
        }

        public async Task UpdateAudit(string receipt,List<string> history,bool isCompleted)
        {
            await auditRepo.UpdateAudit(receipt, history, isCompleted);
        }
    }



    public class OrderAuditRepository : IOrderAuditRepository
    {
        private readonly IMongoCollection<OrderAudit> _auditCollection;
        IMongoClient client = null;
        private readonly IMongoDbContext mongoContext;

        public OrderAuditRepository(IMongoDbContext mongoContext)
        {
            this.mongoContext = mongoContext;
            client = mongoContext.GetMongoClient();
            _auditCollection = mongoContext.GetCollection<OrderAudit>(client, "OrderAudit");
        }

        public async Task<ObjectId> AddAudit(OrderAudit audit)
        {
            await _auditCollection.InsertOneAsync(audit);
            return audit.AuditId;
        }

        public async Task UpdateAudit(string receipt, List<string> audit,bool isCompleted)
        {
            var filter = Builders<OrderAudit>.Filter.Eq(x => x.OrderReceipt, receipt);
            await _auditCollection.FindOneAndUpdateAsync<OrderAudit>(filter,
                Builders<OrderAudit>.Update
                .PushEach<string>(t => t.History, audit)
                .Set(t=>t.IsComplete , isCompleted)
                );
        }
    }
}
