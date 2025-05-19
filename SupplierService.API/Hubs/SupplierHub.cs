using Microsoft.AspNetCore.SignalR;
using SupplierService.Domain.Entities;

namespace SupplierService.API.Hubs
{
    public class SupplierHub : Hub
    {
        public async Task NotifyPurchaseOrderCreated(int purchaseOrderId, int supplierId, string orderNumber)
        {
            await Clients.All.SendAsync("PurchaseOrderCreated", purchaseOrderId, supplierId, orderNumber);
        }

        public async Task NotifyPurchaseOrderStatusChanged(int purchaseOrderId, PurchaseOrderStatus oldStatus, PurchaseOrderStatus newStatus)
        {
            await Clients.All.SendAsync("PurchaseOrderStatusChanged", purchaseOrderId, oldStatus.ToString(), newStatus.ToString());
        }

        public async Task NotifyPurchaseOrderReceived(int purchaseOrderId, int itemId, int receivedQuantity)
        {
            await Clients.All.SendAsync("PurchaseOrderItemReceived", purchaseOrderId, itemId, receivedQuantity);
        }
    }
}
