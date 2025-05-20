namespace MicroservicesVisualizer.Models.Supplier
{
    public class CreatePurchaseOrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
