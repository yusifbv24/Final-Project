using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Hubs
{
    public class InventoryHub : Hub
    {
        private readonly ILogger<InventoryHub> _logger;

        public InventoryHub(ILogger<InventoryHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected to InventoryHub: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected from InventoryHub: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        // Method to send inventory updated event
        public async Task NotifyInventoryUpdated(int inventoryId, int productId, int quantity)
        {
            await Clients.All.SendAsync("InventoryUpdated", inventoryId, productId, quantity);
            _logger.LogInformation("InventoryUpdated event sent: {InventoryId} - Product {ProductId} - Quantity {Quantity}",
                inventoryId, productId, quantity);
        }

        // Method to send inventory transaction created event
        public async Task NotifyInventoryTransactionCreated(int transactionId, int inventoryId, int productId, string type, int quantity)
        {
            await Clients.All.SendAsync("InventoryTransactionCreated", transactionId, inventoryId, productId, type, quantity);
            _logger.LogInformation("InventoryTransactionCreated event sent: {TransactionId} - Type {Type} - Quantity {Quantity}",
                transactionId, type, quantity);
        }
    }
}