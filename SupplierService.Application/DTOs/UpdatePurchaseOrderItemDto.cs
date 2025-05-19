namespace SupplierService.Application.DTOs
{
    public record UpdatePurchaseOrderItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
