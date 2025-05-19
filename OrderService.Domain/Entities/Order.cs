namespace OrderService.Domain.Entities
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        Returned
    }

    public class Order
    {
        public int Id { get; private set; }
        public int CustomerId { get; private set; }
        public OrderStatus Status { get; private set; }
        public decimal TotalAmount { get; private set; }
        public string ShippingAddress { get; private set; } = string.Empty;
        public string BillingAddress { get; private set; } = string.Empty;
        public string? Notes { get; private set; }
        public DateTime OrderDate { get; private set; }
        public DateTime? ShippedDate { get; private set; }
        public DateTime? DeliveredDate { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Navigation property
        public Customer? Customer { get; private set; }
        public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

        // For EF Core
        protected Order() { }

        public Order(
            int customerId,
            string shippingAddress,
            string billingAddress,
            string? notes)
        {
            CustomerId = customerId;
            Status = OrderStatus.Pending;
            ShippingAddress = shippingAddress;
            BillingAddress = billingAddress;
            Notes = notes;
            OrderDate = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            TotalAmount = 0; // Will be calculated when items are added
        }

        public void AddItem(OrderItem item)
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Cannot add items to an order that is not pending");

            Items.Add(item);
            RecalculateTotalAmount();
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveItem(OrderItem item)
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Cannot remove items from an order that is not pending");

            Items.Remove(item);
            RecalculateTotalAmount();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(OrderStatus newStatus)
        {
            // Validate status transitions
            if (!IsValidStatusTransition(Status, newStatus))
                throw new InvalidOperationException($"Invalid status transition from {Status} to {newStatus}");

            Status = newStatus;

            // Update timestamps based on status
            switch (newStatus)
            {
                case OrderStatus.Shipped:
                    ShippedDate = DateTime.UtcNow;
                    break;
                case OrderStatus.Delivered:
                    DeliveredDate = DateTime.UtcNow;
                    break;
            }

            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateShippingAddress(string shippingAddress)
        {
            if (Status != OrderStatus.Pending && Status != OrderStatus.Confirmed)
                throw new InvalidOperationException("Cannot update shipping address for an order that is already being processed");

            ShippingAddress = shippingAddress;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateBillingAddress(string billingAddress)
        {
            if (Status != OrderStatus.Pending && Status != OrderStatus.Confirmed)
                throw new InvalidOperationException("Cannot update billing address for an order that is already being processed");

            BillingAddress = billingAddress;
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

        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                // Valid transitions
                (OrderStatus.Pending, OrderStatus.Confirmed) => true,
                (OrderStatus.Pending, OrderStatus.Cancelled) => true,
                (OrderStatus.Confirmed, OrderStatus.Processing) => true,
                (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
                (OrderStatus.Processing, OrderStatus.Shipped) => true,
                (OrderStatus.Processing, OrderStatus.Cancelled) => true,
                (OrderStatus.Shipped, OrderStatus.Delivered) => true,
                (OrderStatus.Delivered, OrderStatus.Returned) => true,
                // Same status is allowed (no change)
                var (current, next) when current == next => true,
                // All other transitions are invalid
                _ => false
            };
        }
    }
}