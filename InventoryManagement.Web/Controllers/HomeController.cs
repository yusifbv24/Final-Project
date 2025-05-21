using InventoryManagement.Web.Models.Dashboard;
using InventoryManagement.Web.Services.ApiClients;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ProductApiClient _productApiClient;
        private readonly InventoryApiClient _inventoryApiClient;
        private readonly OrderApiClient _orderApiClient;

        public HomeController(
            ILogger<HomeController> logger,
            ProductApiClient productApiClient,
            InventoryApiClient inventoryApiClient,
            OrderApiClient orderApiClient)
        {
            _logger = logger;
            _productApiClient = productApiClient;
            _inventoryApiClient = inventoryApiClient;
            _orderApiClient = orderApiClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var dashboardViewModel = new DashboardViewModel();

                // Get products
                var products = await _productApiClient.GetAllProductsAsync();
                dashboardViewModel.TotalProducts = products.Count;

                // Get locations
                var locations = await _inventoryApiClient.GetAllLocationsAsync();
                dashboardViewModel.TotalLocations = locations.Count;

                // Get orders
                var orders = await _orderApiClient.GetAllOrdersAsync();
                dashboardViewModel.TotalOrders = orders.Count;
                dashboardViewModel.TotalSales = orders.Sum(o => o.TotalAmount);
                dashboardViewModel.RecentOrders = orders.OrderByDescending(o => o.OrderDate).Take(5).ToList();

                // Get inventory
                var inventories = await _inventoryApiClient.GetAllInventoryAsync();
                dashboardViewModel.LowStockItemsCount = inventories.Count(i => i.Quantity < 10);

                // Order status summary
                dashboardViewModel.OrdersByStatus = orders
                    .GroupBy(o => o.Status.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                // Top selling products
                dashboardViewModel.TopSellingProducts = orders
                    .SelectMany(o => o.Items)
                    .GroupBy(i => i.ProductName)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity))
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(5)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                return View(new DashboardViewModel());
            }
        }
    }
}
