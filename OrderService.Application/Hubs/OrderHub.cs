using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Entities;

namespace OrderService.Application.Hubs
{
    public class OrderHub : Hub
    {
        private readonly ILogger<OrderHub> _logger;

        public OrderHub(ILogger<OrderHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected to OrderHub: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected from OrderHub: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        // Method to send order created event
        public async Task NotifyOrderCreated(int orderId, string customerName)
        {
            await Clients.All.SendAsync("OrderCreated", orderId, customerName);
            _logger.LogInformation("OrderCreated event sent: {OrderId} - Customer {CustomerName}", orderId, customerName);
        }

        // Method to send order status changed event
        public async Task NotifyOrderStatusChanged(int orderId, string status)
        {
            await Clients.All.SendAsync("OrderStatusChanged", orderId, status);
            _logger.LogInformation("OrderStatusChanged event sent: {OrderId} - Status {Status}", orderId, status);
        }
    }
}
