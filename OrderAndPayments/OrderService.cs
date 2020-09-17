using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrderAndPayments
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }

        public async Task<ClienteleOrder> SaveOrder(ClienteleOrder order)
        {
            return await orderRepository.Add(order);
        }

        public async Task<ClienteleOrder> AddCaseToOrder(ObjectId order,ObjectId caseId)
        {
            return await orderRepository.AddCaseToOrder(order, caseId);
        }

        public async Task<ClienteleOrder> GetOrderByCaseId(string orderId)
        {
            return await orderRepository.GetOrderByCaseId(orderId);
        }
    }
}
