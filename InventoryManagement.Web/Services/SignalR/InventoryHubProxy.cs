using InventoryManagement.Web.Services.RabbitMQ;
using Microsoft.AspNetCore.SignalR;

namespace InventoryManagement.Web.Services.SignalR
{
    public class InventoryHubProxy : Hub
    {
        private readonly InventoryHubClient _inventoryHubClient;
        private readonly RabbitMQListener _rabbitMQListener;
        private readonly ILogger<InventoryHubProxy> _logger;

        public InventoryHubProxy(
            InventoryHubClient inventoryHubClient,
            RabbitMQListener rabbitMQListener,
            ILogger<InventoryHubProxy> logger)
        {
            _inventoryHubClient = inventoryHubClient;
            _rabbitMQListener = rabbitMQListener;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected to InventoryHub: {ConnectionId}", Context.ConnectionId);

            // Set up event handlers
            _inventoryHubClient.InventoryUpdated += InventoryUpdatedHandler;
            _inventoryHubClient.InventoryTransactionCreated += TransactionCreatedHandler;
            _inventoryHubClient.LowStockAlert += LowStockAlertHandler;
            _rabbitMQListener.MessageReceived += RabbitMQMessageHandler;

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected from InventoryHub: {ConnectionId}", Context.ConnectionId);

            // Clean up event handlers
            _inventoryHubClient.InventoryUpdated -= InventoryUpdatedHandler;
            _inventoryHubClient.InventoryTransactionCreated -= TransactionCreatedHandler;
            _inventoryHubClient.LowStockAlert -= LowStockAlertHandler;
            _rabbitMQListener.MessageReceived -= RabbitMQMessageHandler;

            await base.OnDisconnectedAsync(exception);
        }

        private async void InventoryUpdatedHandler(int inventoryId, int productId, int quantity)
        {
            try
            {
                await Clients.All.SendAsync("InventoryUpdated", inventoryId, productId, quantity);
                _logger.LogInformation("Forwarded InventoryUpdated: {InventoryId} - Product {ProductId} - Quantity {Quantity}",
                    inventoryId, productId, quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding InventoryUpdated");
            }
        }

        private async void TransactionCreatedHandler(int transactionId, int inventoryId, int productId, string type, int quantity)
        {
            try
            {
                await Clients.All.SendAsync("InventoryTransactionCreated", transactionId, inventoryId, productId, type, quantity);
                _logger.LogInformation("Forwarded InventoryTransactionCreated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding InventoryTransactionCreated");
            }
        }

        private async void LowStockAlertHandler(int inventoryId, int productId, int locationId, int quantity, int threshold)
        {
            try
            {
                await Clients.All.SendAsync("LowStockAlert", inventoryId, productId, locationId, quantity, threshold);
                _logger.LogInformation("Forwarded LowStockAlert");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding LowStockAlert");
            }
        }

        private async void RabbitMQMessageHandler(string routingKey, string message)
        {
            if (routingKey.StartsWith("inventory."))
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
