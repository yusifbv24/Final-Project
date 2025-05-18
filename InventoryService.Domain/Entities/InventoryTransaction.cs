namespace InventoryService.Domain.Entities
{
    public enum TransactionType
    {
        StockIn,
        StockOut,
        Adjustment,
        Transfer
    }

    public class InventoryTransaction
    {
        public int Id { get; private set; }
        public int InventoryId { get; private set; }
        public TransactionType Type { get; private set; }
        public int Quantity { get; private set; }
        public string Reference { get; private set; } = string.Empty;
        public string Notes { get; private set; } = string.Empty;
        public DateTime TransactionDate { get; private set; }

        // Navigation property
        public Inventory? Inventory { get; private set; }

        // For EF Core
        protected InventoryTransaction() { }

        public InventoryTransaction(int inventoryId, TransactionType type, int quantity, string reference, string notes)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            InventoryId = inventoryId;
            Type = type;
            Quantity = quantity;
            Reference = reference;
            Notes = notes;
            TransactionDate = DateTime.UtcNow;
        }
    }
}
