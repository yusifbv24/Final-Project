using MicroservicesVisualizer.Models.Dashboard;
using MicroservicesVisualizer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MicroservicesVisualizer.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ISupplierService _supplierService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IInventoryService inventoryService,
            IOrderService orderService,
            IProductService productService,
            ISupplierService supplierService,
            ILogger<DashboardController> logger)
        {
            _inventoryService = inventoryService;
            _orderService = orderService;
            _productService = productService;
            _supplierService = supplierService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = new DashboardViewModel();

                // Get inventory data
                var inventories = await _inventoryService.GetAllInventoryAsync();
                var locations = await _inventoryService.GetAllLocationsAsync();
                var lowStockItems = await _inventoryService.GetLowStockItemsAsync();
                var transactionSummary = await _inventoryService.GetTransactionSummaryAsync(DateTime.UtcNow.AddDays(-7), null);

                viewModel.TotalInventoryItems = inventories.Count();
                viewModel.TotalLocations = locations.Count();
                viewModel.TotalStockQuantity = inventories.Sum(i => i.Quantity);
                viewModel.LowStockItems = lowStockItems.Take(5);
                viewModel.RecentTransactions = transactionSummary;

                // Get order data
                var orders = await _orderService.GetAllOrdersAsync();
                var orderStats = await _orderService.GetOrderStatisticsAsync();

                viewModel.TotalOrders = orders.Count();
                viewModel.PendingOrders = orders.Count(o => o.Status == Models.Order.OrderStatus.Pending);
                viewModel.TotalOrderValue = orders.Sum(o => o.TotalAmount);
                viewModel.OrderStatistics = orderStats;
                viewModel.RecentOrders = orders.OrderByDescending(o => o.OrderDate).Take(5);

                // Get product data
                var products = await _productService.GetAllProductsAsync();
                var categories = await _productService.GetAllCategoriesAsync();

                viewModel.TotalProducts = products.Count();
                viewModel.TotalCategories = categories.Count();
                viewModel.TopProducts = products.OrderByDescending(p => p.Price).Take(5);

                // Get supplier data
                var suppliers = await _supplierService.GetAllSuppliersAsync();
                var purchaseOrders = await _supplierService.GetAllPurchaseOrdersAsync();
                var poSummary = await _supplierService.GetPurchaseOrderSummaryAsync(DateTime.UtcNow.AddDays(-30), null);

                viewModel.TotalSuppliers = suppliers.Count();
                viewModel.ActiveSuppliers = suppliers.Count(s => s.IsActive);
                viewModel.TotalPurchaseOrders = purchaseOrders.Count();
                viewModel.PendingPurchaseOrders = purchaseOrders.Count(po =>
                    po.Status == Models.Supplier.PurchaseOrderStatus.Draft ||
                    po.Status == Models.Supplier.PurchaseOrderStatus.Submitted);
                viewModel.PurchaseOrderSummary = poSummary;
                viewModel.RecentPurchaseOrders = purchaseOrders.OrderByDescending(po => po.OrderDate).Take(5);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                // Return a view with empty data to avoid crashing
                return View(new DashboardViewModel());
            }
        }
    }
}
