using Fundamentals.DbContext;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrderAndPayments
{
    public class OrderRepository : IOrderRepository
    {
        IMongoClient client = null;
        private readonly IMongoDbContext mongoContext;
        private readonly IMongoCollection<ClienteleOrder> _orderCollection;
        public OrderRepository(IMongoDbContext mongoContext)
        {
            this.mongoContext = mongoContext;
            client = mongoContext.GetMongoClient();
            _orderCollection = mongoContext.GetCollection<ClienteleOrder>(client, "ClientOrder");
        }

        public async Task<ClienteleOrder> Add(ClienteleOrder order)
        {
            await _orderCollection.InsertOneAsync(order);
            return order;
        }
        public async Task<ClienteleOrder> AddCaseToOrder(ObjectId orderId, ObjectId CaseId)
        {
            var filter = Builders<ClienteleOrder>.Filter.Eq(t => t.ClientOrderId, orderId);
            var updatedDoc = await _orderCollection.FindOneAndUpdateAsync<ClienteleOrder>(
               filter,
               Builders<ClienteleOrder>.Update.Set(
                   t => t.CaseId,
                   CaseId)
               );
            return updatedDoc;
        }

        public async Task<ClienteleOrder> AddPaymentToOrder(string orderId, string paymentId)
        {
            ObjectId orderObjectId = ObjectId.Parse(orderId);
            ObjectId paymentObjectId = ObjectId.Parse(paymentId);
            var filter = Builders<ClienteleOrder>.Filter.Eq(t => t.ClientOrderId, orderObjectId);
            var updatedDoc = await _orderCollection.FindOneAndUpdateAsync<ClienteleOrder>(
               filter,
               Builders<ClienteleOrder>.Update.Set(
                   t => t.ClientelePaymentId,
                   paymentObjectId)
               );
            return updatedDoc;
        }

        public async Task<ClienteleOrder> GetOrderByCaseId(string caseId)
        {
            ObjectId orderObjectId = ObjectId.Parse(caseId);
            var filter = Builders<ClienteleOrder>.Filter.Eq(t => t.CaseId, orderObjectId);
            var fullOrder = await _orderCollection.FindAsync<ClienteleOrder>(filter);
            return fullOrder.FirstOrDefault();
        }

        public async Task<ClienteleOrder> GetOrderById(string orderId)
        {
            ObjectId orderObjectId = ObjectId.Parse(orderId);
            var filter = Builders<ClienteleOrder>.Filter.Eq(t => t.ClientOrderId, orderObjectId);
            var fullOrder = await _orderCollection.FindAsync<ClienteleOrder>(filter);
            return fullOrder.FirstOrDefault();
        }

        public async Task<ClienteleOrder> GetOrderByReceipt(string receipt)
        {
            var filter = Builders<ClienteleOrder>.Filter.Eq(t => t.Receipt, receipt);
            var fullOrder = await _orderCollection.FindAsync<ClienteleOrder>(filter);
            return fullOrder.FirstOrDefault();
        }

        public async Task<List<ClienteleOrder>> GetOrderOfUser(string userId)
        {
            ObjectId userObjetId = ObjectId.Parse(userId);
            var filter = Builders<ClienteleOrder>.Filter.Eq(t => t.ClientId, userObjetId);
            return await _orderCollection.Find<ClienteleOrder>(filter).SortByDescending(x=>x.OrderPlacedOn).ToListAsync();
        }


    }
}
