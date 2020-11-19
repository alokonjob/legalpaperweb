using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fundamentals.DbContext;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
namespace OrderAndPayments
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IMongoCollection<ClientelePayment> _paymentCollection;
        IMongoClient client = null;
        private readonly IMongoDbContext mongoContext;

        public PaymentRepository(IMongoDbContext mongoContext)
        {
            this.mongoContext = mongoContext;
            client = mongoContext.GetMongoClient();
            _paymentCollection = mongoContext.GetCollection<ClientelePayment>(client, "ClientPayments");
            
        }


        public async Task<ObjectId> SavePaymentAsync(ClientelePayment clientPayment)
        {
            await _paymentCollection.InsertOneAsync(clientPayment);
            return clientPayment.PaymentId;
        }

        public async Task<ClientelePayment> UpdatePayment(ObjectId paymentId, ObjectId OrderId , string status)
        {
            var filter = Builders<ClientelePayment>.Filter.Eq(t => t.PaymentId, paymentId);
            var updatedDoc = await _paymentCollection.FindOneAndUpdateAsync<ClientelePayment>(
               filter,
               Builders<ClientelePayment>.Update
               .Set(t => t.ClienteleOrderId,OrderId)
               .Set(x=>x.PaymentStatus,status)
               );
            return updatedDoc;
        }

        public async Task<ClientelePayment> GetPaymentByOrderId(string OrderId)
        {
            ObjectId OrderObjectId = ObjectId.Parse(OrderId);
            var filter = Builders<ClientelePayment>.Filter.Eq(t => t.ClienteleOrderId, OrderObjectId);
            var updatedDoc = await _paymentCollection.FindAsync<ClientelePayment>(filter);
            return updatedDoc.FirstOrDefault();
        }

        public async Task<List<ClientelePayment>> GetPaymentByOrderId(List<ObjectId> orderIds)
        {
            var filter = Builders<ClientelePayment>.Filter.In(t => t.ClienteleOrderId, orderIds);
            var updatedDoc = await _paymentCollection.FindAsync<ClientelePayment>(filter);
            return await updatedDoc.ToListAsync();
        }



    }
}
