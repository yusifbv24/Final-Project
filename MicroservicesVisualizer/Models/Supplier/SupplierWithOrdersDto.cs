namespace MicroservicesVisualizer.Models.Supplier
{
    public class SupplierWithOrdersDto : SupplierDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        public IEnumerable<PurchaseOrderDto> RecentOrders { get; set; } = new List<PurchaseOrderDto>();
    }
}
