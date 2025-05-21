namespace OrderService.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; private set; }
        public int OrderId { get; private set; }
        public int ProductId { get; private set; }
        public string ProductName { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }

        // Navigation property
        public Order? Order { get; private set; }

        // For EF Core
        protected OrderItem() { }

        public OrderItem(int orderId, int productId, string productName, decimal price, int quantity)
        {
            if (price < 0)
                throw new ArgumentException("Price cannot be negative", nameof(price));

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            OrderId = orderId;
            ProductId = productId;
            ProductName = productName;
            Price = price;
            Quantity = quantity;
        }

        public void UpdateQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            Quantity = quantity;
        }
    }
}
