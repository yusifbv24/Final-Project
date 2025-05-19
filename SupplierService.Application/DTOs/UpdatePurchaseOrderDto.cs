namespace SupplierService.Application.DTOs
{
    public record UpdatePurchaseOrderDto
    {
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public List<UpdatePurchaseOrderItemDto> Items { get; set; } = new List<UpdatePurchaseOrderItemDto>();
    }
}
