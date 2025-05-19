namespace SupplierService.Domain.Entities
{
    public class PurchaseOrderItem
    {
        public int Id { get; private set; }
        public int PurchaseOrderId { get; private set; }
        public int ProductId { get; private set; }
        public string ProductName { get; private set; } = string.Empty;
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public int ReceivedQuantity { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Navigation property
        public PurchaseOrder? PurchaseOrder { get; private set; }

        // For EF Core
        protected PurchaseOrderItem() { }

        public PurchaseOrderItem(
            int purchaseOrderId,
            int productId,
            string productName,
            int quantity,
            decimal unitPrice)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            if (unitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

            PurchaseOrderId = purchaseOrderId;
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
            ReceivedQuantity = 0;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            if (PurchaseOrder != null && PurchaseOrder.Status != PurchaseOrderStatus.Draft)
                throw new InvalidOperationException("Cannot update quantity of an item in a purchase order that is not in draft status");

            Quantity = quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateUnitPrice(decimal unitPrice)
        {
            if (unitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

            if (PurchaseOrder != null && PurchaseOrder.Status != PurchaseOrderStatus.Draft)
                throw new InvalidOperationException("Cannot update unit price of an item in a purchase order that is not in draft status");

            UnitPrice = unitPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ReceiveItems(int receivedQuantity)
        {
            if (receivedQuantity <= 0)
                throw new ArgumentException("Received quantity must be positive", nameof(receivedQuantity));

            if (ReceivedQuantity + receivedQuantity > Quantity)
                throw new InvalidOperationException($"Cannot receive more than the ordered quantity. Ordered: {Quantity}, Already received: {ReceivedQuantity}, Trying to receive: {receivedQuantity}");

            if (PurchaseOrder != null && PurchaseOrder.Status != PurchaseOrderStatus.Ordered && PurchaseOrder.Status != PurchaseOrderStatus.PartiallyReceived)
                throw new InvalidOperationException("Cannot receive items for a purchase order that is not in ordered or partially received status");

            ReceivedQuantity += receivedQuantity;
            UpdatedAt = DateTime.UtcNow;

            // Update the purchase order status if needed
            if (PurchaseOrder != null)
            {
                bool allItemsReceived = PurchaseOrder.Items.All(item => item.ReceivedQuantity >= item.Quantity);
                bool someItemsReceived = PurchaseOrder.Items.Any(item => item.ReceivedQuantity > 0);

                if (allItemsReceived && PurchaseOrder.Status != PurchaseOrderStatus.Completed)
                    PurchaseOrder.UpdateStatus(PurchaseOrderStatus.Completed);
                else if (someItemsReceived && !allItemsReceived && PurchaseOrder.Status != PurchaseOrderStatus.PartiallyReceived)
                    PurchaseOrder.UpdateStatus(PurchaseOrderStatus.PartiallyReceived);
            }
        }
    }
}