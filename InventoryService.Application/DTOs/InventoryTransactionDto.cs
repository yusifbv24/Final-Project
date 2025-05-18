using InventoryService.Domain.Entities;

namespace InventoryService.Application.DTOs
{
    public record InventoryTransactionDto
    {
        public int Id { get; init; }
        public int InventoryId { get; init; }
        public TransactionType Type { get; init; }
        public int Quantity { get; init; }
        public string Reference { get; init; } = string.Empty;
        public string Notes { get; init; } = string.Empty;
        public DateTime TransactionDate { get; init; }
    }
}