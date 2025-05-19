using SupplierService.Domain.Entities;

namespace SupplierService.Application.Events
{
    public record PurchaseOrderStatusChangedEvent
    {
        public int PurchaseOrderId { get; init; }
        public string OrderNumber { get; init; } = string.Empty;
        public PurchaseOrderStatus OldStatus { get; init; }
        public PurchaseOrderStatus NewStatus { get; init; }
        public DateTime ChangedAt { get; init; }
    }
}
