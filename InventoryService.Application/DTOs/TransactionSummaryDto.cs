namespace InventoryService.Application.DTOs
{
    public record TransactionSummaryDto
    {
        public int TotalTransactions { get; set; }
        public int StockInTotal { get; set; }
        public int StockOutTotal { get; set; }
        public int AdjustmentTotal { get; set; }
        public int TransferTotal { get; set; }
        public Dictionary<string, int> TransactionsByType { get; set; } = new();
    }
}
