using System.Text.Json.Serialization;

namespace MicroservicesVisualizer.Models.Inventory
{
    public enum TransactionType
    {
        StockIn,
        StockOut,
        Adjustment,
        Transfer
    }
    public class InventoryTransactionDto
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TransactionType Type { get; set; }
        public int Quantity { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
    }
}
