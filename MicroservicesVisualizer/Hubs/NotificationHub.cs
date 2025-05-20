using MicroservicesVisualizer.Models.Inventory;
using MicroservicesVisualizer.Models.Order;
using MicroservicesVisualizer.Models.Supplier;
using Microsoft.AspNetCore.SignalR;

namespace MicroservicesVisualizer.Hubs
{
    public class NotificationHub : Hub
    {
        // Inventory notifications
        public async Task SendInventoryUpdated(int inventoryId, int productId, int quantity)
        {
            await Clients.All.SendAsync("InventoryUpdated", inventoryId, productId, quantity);
        }

        public async Task SendInventoryTransactionCreated(int transactionId, int inventoryId, int productId, TransactionType type, int quantity)
        {
            await Clients.All.SendAsync("InventoryTransactionCreated", transactionId, inventoryId, productId, type, quantity);
        }

        public async Task SendLowStockAlert(int inventoryId, int productId, int locationId, int quantity, int threshold)
        {
            await Clients.All.SendAsync("LowStockAlert", inventoryId, productId, locationId, quantity, threshold);
        }

        // Order notifications
        public async Task SendOrderCreated(int orderId, int customerId, decimal totalAmount)
        {
            await Clients.All.SendAsync("OrderCreated", orderId, customerId, totalAmount);
        }

        public async Task SendOrderStatusChanged(int orderId, OrderStatus oldStatus, OrderStatus newStatus)
        {
            await Clients.All.SendAsync("OrderStatusChanged", orderId, oldStatus.ToString(), newStatus.ToString());
        }

        public async Task SendOrderCancelled(int orderId)
        {
            await Clients.All.SendAsync("OrderCancelled", orderId);
        }

        // Product notifications
        public async Task SendProductCreated(int productId, string name)
        {
            await Clients.All.SendAsync("ProductCreated", productId, name);
        }

        public async Task SendProductUpdated(int productId, string name)
        {
            await Clients.All.SendAsync("ProductUpdated", productId, name);
        }

        public async Task SendProductDeleted(int productId)
        {
            await Clients.All.SendAsync("ProductDeleted", productId);
        }

        // Supplier notifications
        public async Task SendPurchaseOrderCreated(int purchaseOrderId, int supplierId, string orderNumber)
        {
            await Clients.All.SendAsync("PurchaseOrderCreated", purchaseOrderId, supplierId, orderNumber);
        }

        public async Task SendPurchaseOrderStatusChanged(int purchaseOrderId, PurchaseOrderStatus oldStatus, PurchaseOrderStatus newStatus)
        {
            await Clients.All.SendAsync("PurchaseOrderStatusChanged", purchaseOrderId, oldStatus.ToString(), newStatus.ToString());
        }

        public async Task SendPurchaseOrderItemReceived(int purchaseOrderId, int itemId, int receivedQuantity)
        {
            await Clients.All.SendAsync("PurchaseOrderItemReceived", purchaseOrderId, itemId, receivedQuantity);
        }
    }
}
