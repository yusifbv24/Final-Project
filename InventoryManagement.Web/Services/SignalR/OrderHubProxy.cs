using System.Collections.Concurrent;
using InventoryManagement.Web.Services.RabbitMQ;
using Microsoft.AspNetCore.SignalR;

namespace InventoryManagement.Web.Services.SignalR
{
    public class OrderHubProxy : Hub
    {
        private readonly OrderHubClient _orderHubClient;
        private readonly RabbitMQListener _rabbitMQListener;
        private readonly ILogger<OrderHubProxy> _logger;

        public OrderHubProxy(
            OrderHubClient orderHubClient,
            RabbitMQListener rabbitMQListener,
            ILogger<OrderHubProxy> logger)
        {
            _orderHubClient = orderHubClient;
            _rabbitMQListener = rabbitMQListener;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected to OrderHub: {ConnectionId}", Context.ConnectionId);

            // Set up event handlers
            _orderHubClient.OrderCreated += OrderCreatedHandler;
            _orderHubClient.OrderStatusChanged += OrderStatusChangedHandler;
            _rabbitMQListener.MessageReceived += RabbitMQMessageHandler;

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected from OrderHub: {ConnectionId}", Context.ConnectionId);

            // Clean up event handlers
            _orderHubClient.OrderCreated -= OrderCreatedHandler;
            _orderHubClient.OrderStatusChanged -= OrderStatusChangedHandler;
            _rabbitMQListener.MessageReceived -= RabbitMQMessageHandler;

            await base.OnDisconnectedAsync(exception);
        }

        private async void OrderCreatedHandler(int orderId, string customerName)
        {
            try
            {
                await Clients.All.SendAsync("OrderCreated", orderId, customerName);
                _logger.LogInformation("Forwarded OrderCreated: {OrderId} - {CustomerName}", orderId, customerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding OrderCreated");
            }
        }

        private async void OrderStatusChangedHandler(int orderId, string status)
        {
            try
            {
                await Clients.All.SendAsync("OrderStatusChanged", orderId, status);
                _logger.LogInformation("Forwarded OrderStatusChanged: {OrderId} - {Status}", orderId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding OrderStatusChanged");
            }
        }

        private async void RabbitMQMessageHandler(string routingKey, string message)
        {
            if (routingKey.StartsWith("order."))
            {
                try
                {
                    await Clients.All.SendAsync("MessageReceived", routingKey, message);
                    _logger.LogInformation("Forwarded RabbitMQ message: {RoutingKey}", routingKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error forwarding RabbitMQ message");
                }
            }
        }
    }
}
