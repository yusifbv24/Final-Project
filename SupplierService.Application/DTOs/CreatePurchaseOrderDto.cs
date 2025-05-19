namespace SupplierService.Application.DTOs
{
    public record CreatePurchaseOrderDto
    {
        public int SupplierId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public List<CreatePurchaseOrderItemDto> Items { get; set; } = new List<CreatePurchaseOrderItemDto>();
    }
}
