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

        public async Task<ClienteleOrder> GetOrderByCaseId(string caseId)
        {
            return await orderRepository.GetOrderByCaseId(caseId);
        }

        public async Task<ClienteleOrder> GetOrderById(string orderId)
        {
            return await orderRepository.GetOrderById(orderId);
        }

        public async Task<ClienteleOrder> GetOrderByReceipt(string receipt)
        {
            return await orderRepository.GetOrderByReceipt(receipt);
        }

        public async Task<List<ClienteleOrder>> GetOrdersOFUser(string userId)
        {
            return await orderRepository.GetOrderOfUser(userId);
        }

        public async Task<List<ClienteleOrder>> GetCustomOrders(string Email = "")
        {
            return await orderRepository.GetCustomOrders(Email);
        }

        public async Task<ClienteleOrder> AddPaymentToOrder(string orderId , string paymentId, OrderStatus status = OrderStatus.PaymentCompletedSuccess)
        {
            return await orderRepository.AddPaymentToOrder(orderId, paymentId, status);
        }

        public async Task<ClienteleOrder> UpdateOrderStatus(string orderId, OrderStatus status)
        {
           return await orderRepository.UpdateOrderStatus(orderId, status);
        }

        public async Task<ClienteleOrder> UpdateOrderStatusByReceipt(string receipt, OrderStatus status)
        {
            return await orderRepository.UpdateOrderStatusByReceipt(receipt, status);
        }

        public async Task<ClienteleOrder> UpdateCustomerCost(string orderId, double cost)
        {
            return await orderRepository.UpdateCustomerCost(orderId, cost);
        }


    }
}
