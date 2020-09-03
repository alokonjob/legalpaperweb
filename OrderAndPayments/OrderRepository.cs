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

    }
}
