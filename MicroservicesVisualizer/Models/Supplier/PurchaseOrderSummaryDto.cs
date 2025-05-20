namespace MicroservicesVisualizer.Models.Supplier
{
    public class PurchaseOrderSummaryDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageOrderAmount { get; set; }
        public Dictionary<int, int> OrdersBySupplier { get; set; } = new();
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        public Dictionary<string, int> OrdersByMonth { get; set; } = new();
    }
}
