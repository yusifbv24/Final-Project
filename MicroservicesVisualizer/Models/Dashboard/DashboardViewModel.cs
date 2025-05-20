using MicroservicesVisualizer.Models.Inventory;
using MicroservicesVisualizer.Models.Order;
using MicroservicesVisualizer.Models.Product;
using MicroservicesVisualizer.Models.Supplier;

namespace MicroservicesVisualizer.Models.Dashboard
{
    public class DashboardViewModel
    {
        // Inventory metrics
        public int TotalInventoryItems { get; set; }
        public int TotalLocations { get; set; }
        public int TotalStockQuantity { get; set; }
        public IEnumerable<LowStockItemDto> LowStockItems { get; set; } = new List<LowStockItemDto>();
        public TransactionSummaryDto RecentTransactions { get; set; } = new TransactionSummaryDto();

        // Order metrics
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalOrderValue { get; set; }
        public OrderStatisticsDto OrderStatistics { get; set; } = new OrderStatisticsDto();
        public IEnumerable<OrderDto> RecentOrders { get; set; } = new List<OrderDto>();

        // Product metrics
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public IEnumerable<ProductDto> TopProducts { get; set; } = new List<ProductDto>();

        // Supplier metrics
        public int TotalSuppliers { get; set; }
        public int ActiveSuppliers { get; set; }
        public int TotalPurchaseOrders { get; set; }
        public int PendingPurchaseOrders { get; set; }
        public PurchaseOrderSummaryDto PurchaseOrderSummary { get; set; } = new PurchaseOrderSummaryDto();
        public IEnumerable<PurchaseOrderDto> RecentPurchaseOrders { get; set; } = new List<PurchaseOrderDto>();
    }
}
