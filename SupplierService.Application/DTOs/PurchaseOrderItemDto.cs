namespace SupplierService.Application.DTOs
{
    public record PurchaseOrderItemDto
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int ReceivedQuantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}
