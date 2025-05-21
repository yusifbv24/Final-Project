namespace OrderService.Domain.Entities
{
    public class Order
    {
        public int Id { get; private set; }
        public string CustomerName { get; private set; } = string.Empty;
        public string CustomerEmail { get; private set; } = string.Empty;
        public string ShippingAddress { get; private set; } = string.Empty;
        public OrderStatus Status { get; private set; }
        public decimal TotalAmount { get; private set; }
        public DateTime OrderDate { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Navigation property
        public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

        // For EF Core
        protected Order() { }

        public Order(string customerName, string customerEmail, string shippingAddress)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Customer name cannot be empty", nameof(customerName));

            if (string.IsNullOrWhiteSpace(customerEmail))
                throw new ArgumentException("Customer email cannot be empty", nameof(customerEmail));

            if (string.IsNullOrWhiteSpace(shippingAddress))
                throw new ArgumentException("Shipping address cannot be empty", nameof(shippingAddress));

            CustomerName = customerName;
            CustomerEmail = customerEmail;
            ShippingAddress = shippingAddress;
            Status = OrderStatus.Pending;
            OrderDate = DateTime.UtcNow;
            TotalAmount = 0;
        }

        public void UpdateStatus(OrderStatus status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddOrderItem(OrderItem item)
        {
            OrderItems.Add(item);
            RecalculateTotalAmount();
        }

        public void RemoveOrderItem(OrderItem item)
        {
            OrderItems.Remove(item);
            RecalculateTotalAmount();
        }

        private void RecalculateTotalAmount()
        {
            TotalAmount = OrderItems.Sum(item => item.Price * item.Quantity);
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateShippingAddress(string shippingAddress)
        {
            if (string.IsNullOrWhiteSpace(shippingAddress))
                throw new ArgumentException("Shipping address cannot be empty", nameof(shippingAddress));

            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Cannot update shipping address after order processing has begun");

            ShippingAddress = shippingAddress;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}