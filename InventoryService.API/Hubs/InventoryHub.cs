using Microsoft.AspNetCore.SignalR;

namespace InventoryService.API.Hubs
{
    public class InventoryHub : Hub
    {
        public async Task NotifyInventoryUpdated(int inventoryId, int productId, int quantity)
        {
            await Clients.All.SendAsync("InventoryUpdated", inventoryId, productId, quantity);
        }

        public async Task NotifyInventoryTransactionCreated(int transactionId, int inventoryId, int productId, string type, int quantity)
        {
            await Clients.All.SendAsync("InventoryTransactionCreated", transactionId, inventoryId, productId, type, quantity);
        }

        public async Task NotifyLowStockAlert(int inventoryId, int productId, int locationId, int quantity, int threshold)
        {
            await Clients.All.SendAsync("LowStockAlert", inventoryId, productId, locationId, quantity, threshold);
        }
    }
}