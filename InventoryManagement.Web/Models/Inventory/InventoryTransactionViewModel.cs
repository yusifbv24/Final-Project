namespace InventoryManagement.Web.Models.Inventory
{
    public record InventoryTransactionViewModel
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
    }
}
