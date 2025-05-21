using InventoryManagement.Web.Models.Order;

namespace InventoryManagement.Web.Models.Dashboard
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalLocations { get; set; }
        public int TotalOrders { get; set; }
        public int LowStockItemsCount { get; set; }
        public decimal TotalSales { get; set; }
        public List<OrderViewModel> RecentOrders { get; set; } = new List<OrderViewModel>();
        public Dictionary<string, int> OrdersByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> TopSellingProducts { get; set; } = new Dictionary<string, int>();
    }
}
