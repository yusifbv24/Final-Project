namespace InventoryService.Domain.Entities
{
    public class Inventory
    {
        public int Id { get; private set; }
        public int ProductId { get; private set; }
        public int LocationId { get; private set; }
        public int Quantity { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Navigation properties
        public Location? Location { get; private set; }
        public ICollection<InventoryTransaction> Transactions { get; private set; } = new List<InventoryTransaction>();

        // For EF Core
        protected Inventory() { }

        public Inventory(int productId, int locationId, int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

            ProductId = productId;
            LocationId = locationId;
            Quantity = quantity;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity < 0)
                throw new ArgumentException("Quantity cannot be negative", nameof(newQuantity));

            Quantity = newQuantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddStock(int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive", nameof(amount));

            Quantity += amount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveStock(int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive", nameof(amount));

            if (amount > Quantity)
                throw new InvalidOperationException("Not enough stock available");

            Quantity -= amount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
