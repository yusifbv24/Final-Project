using SupplierService.Domain.Entities;

namespace SupplierService.Application.Events
{
    public record PurchaseOrderCreatedEvent
    {
        public int PurchaseOrderId { get; init; }
        public int SupplierId { get; init; }
        public string OrderNumber { get; init; } = string.Empty;
        public PurchaseOrderStatus Status { get; init; }
        public decimal TotalAmount { get; init; }
        public DateTime OrderDate { get; init; }
        public IEnumerable<PurchaseOrderItemEvent> Items { get; init; } = new List<PurchaseOrderItemEvent>();
    }

    public record PurchaseOrderItemEvent
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
    }
}
