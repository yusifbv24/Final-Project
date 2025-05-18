using InventoryService.Domain.Entities;

namespace InventoryService.Application.Events
{
    public record InventoryTransactionCreatedEvent
    {
        public int TransactionId { get; init; }
        public int InventoryId { get; init; }
        public int ProductId { get; init; }
        public int LocationId { get; init; }
        public TransactionType Type { get; init; }
        public int Quantity { get; init; }
        public DateTime TransactionDate { get; init; }
    }
}
