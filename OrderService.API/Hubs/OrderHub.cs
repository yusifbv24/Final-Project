using Microsoft.AspNetCore.SignalR;
using OrderService.Domain.Entities;

namespace OrderService.API.Hubs
{
    public class OrderHub : Hub
    {
        public async Task NotifyOrderCreated(int orderId, int customerId, decimal totalAmount)
        {
            await Clients.All.SendAsync("OrderCreated", orderId, customerId, totalAmount);
        }

        public async Task NotifyOrderStatusChanged(int orderId, OrderStatus oldStatus, OrderStatus newStatus)
        {
            await Clients.All.SendAsync("OrderStatusChanged", orderId, oldStatus.ToString(), newStatus.ToString());
        }

        public async Task NotifyOrderCancelled(int orderId)
        {
            await Clients.All.SendAsync("OrderCancelled", orderId);
        }
    }
}
