namespace SupplierService.Application.Events
{
    public record PurchaseOrderItemReceivedEvent
    {
        public int PurchaseOrderId { get; init; }
        public string OrderNumber { get; init; } = string.Empty;
        public int PurchaseOrderItemId { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int ReceivedQuantity { get; init; }
        public int TotalReceivedQuantity { get; init; }
        public int OrderedQuantity { get; init; }
        public DateTime ReceivedAt { get; init; }
    }
}
