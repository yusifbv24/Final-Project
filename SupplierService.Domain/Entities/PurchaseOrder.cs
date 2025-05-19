namespace SupplierService.Domain.Entities
{
    public enum PurchaseOrderStatus
    {
        Draft,
        Submitted,
        Approved,
        Rejected,
        Ordered,
        PartiallyReceived,
        Completed,
        Cancelled
    }

    public class PurchaseOrder
    {
        public int Id { get; private set; }
        public int SupplierId { get; private set; }
        public string OrderNumber { get; private set; } = string.Empty;
        public PurchaseOrderStatus Status { get; private set; }
        public decimal TotalAmount { get; private set; }
        public DateTime OrderDate { get; private set; }
        public DateTime? ExpectedDeliveryDate { get; private set; }
        public string? Notes { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Navigation properties
        public Supplier? Supplier { get; private set; }
        public ICollection<PurchaseOrderItem> Items { get; private set; } = new List<PurchaseOrderItem>();

        // For EF Core
        protected PurchaseOrder() { }

        public PurchaseOrder(
            int supplierId,
            string orderNumber,
            DateTime? expectedDeliveryDate,
            string? notes)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
                throw new ArgumentException("Order number cannot be empty", nameof(orderNumber));

            SupplierId = supplierId;
            OrderNumber = orderNumber;
            Status = PurchaseOrderStatus.Draft;
            TotalAmount = 0; // Will be calculated when items are added
            OrderDate = DateTime.UtcNow;
            ExpectedDeliveryDate = expectedDeliveryDate;
            Notes = notes;
            CreatedAt = DateTime.UtcNow;
        }

        public void AddItem(PurchaseOrderItem item)
        {
            if (Status != PurchaseOrderStatus.Draft)
                throw new InvalidOperationException("Cannot add items to a purchase order that is not in draft status");

            Items.Add(item);
            RecalculateTotalAmount();
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveItem(PurchaseOrderItem item)
        {
            if (Status != PurchaseOrderStatus.Draft)
                throw new InvalidOperationException("Cannot remove items from a purchase order that is not in draft status");

            Items.Remove(item);
            RecalculateTotalAmount();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(PurchaseOrderStatus newStatus)
        {
            // Validate status transitions
            if (!IsValidStatusTransition(Status, newStatus))
                throw new InvalidOperationException($"Invalid status transition from {Status} to {newStatus}");

            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateExpectedDeliveryDate(DateTime? expectedDeliveryDate)
        {
            ExpectedDeliveryDate = expectedDeliveryDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateNotes(string? notes)
        {
            Notes = notes;
            UpdatedAt = DateTime.UtcNow;
        }

        private void RecalculateTotalAmount()
        {
            TotalAmount = Items.Sum(item => item.UnitPrice * item.Quantity);
        }

        private bool IsValidStatusTransition(PurchaseOrderStatus currentStatus, PurchaseOrderStatus newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                // Valid transitions
                (PurchaseOrderStatus.Draft, PurchaseOrderStatus.Submitted) => true,
                (PurchaseOrderStatus.Draft, PurchaseOrderStatus.Cancelled) => true,
                (PurchaseOrderStatus.Submitted, PurchaseOrderStatus.Approved) => true,
                (PurchaseOrderStatus.Submitted, PurchaseOrderStatus.Rejected) => true,
                (PurchaseOrderStatus.Submitted, PurchaseOrderStatus.Cancelled) => true,
                (PurchaseOrderStatus.Approved, PurchaseOrderStatus.Ordered) => true,
                (PurchaseOrderStatus.Approved, PurchaseOrderStatus.Cancelled) => true,
                (PurchaseOrderStatus.Ordered, PurchaseOrderStatus.PartiallyReceived) => true,
                (PurchaseOrderStatus.Ordered, PurchaseOrderStatus.Completed) => true,
                (PurchaseOrderStatus.Ordered, PurchaseOrderStatus.Cancelled) => true,
                (PurchaseOrderStatus.PartiallyReceived, PurchaseOrderStatus.Completed) => true,
                (PurchaseOrderStatus.PartiallyReceived, PurchaseOrderStatus.Cancelled) => true,
                // Same status is allowed (no change)
                var (current, next) when current == next => true,
                // All other transitions are invalid
                _ => false
            };
        }
    }
}
