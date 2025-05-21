using Microsoft.AspNetCore.SignalR;
using OrderService.Domain.Entities;

namespace OrderService.API.Hubs
{
    public class OrderHub : Hub
    {
        public async Task NotifyOrderCreated(int orderId, string customerName)
        {
            await Clients.All.SendAsync("OrderCreated", orderId, customerName);
        }

        public async Task NotifyOrderStatusChanged(int orderId, OrderStatus status)
        {
            await Clients.All.SendAsync("OrderStatusChanged", orderId, status.ToString());
        }
    }
}
