using InventoryManagement.Web.Services.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace InventoryManagement.Web.Controllers
{
    public class SignalRTestController : Controller
    {
        private readonly IHubContext<ProductHubProxy> _productHubContext;
        private readonly IHubContext<OrderHubProxy> _orderHubContext;
        private readonly IHubContext<InventoryHubProxy> _inventoryHubContext;
        private readonly ProductHubClient _productHubClient;
        private readonly InventoryHubClient _inventoryHubClient;
        private readonly OrderHubClient _orderHubClient;
        private readonly ILogger<SignalRTestController> _logger;

        public SignalRTestController(
            IHubContext<ProductHubProxy> productHubContext,
            IHubContext<OrderHubProxy> orderHubContext,
            IHubContext<InventoryHubProxy> inventoryHubContext,
            ProductHubClient productHubClient,
            InventoryHubClient inventoryHubClient,
            OrderHubClient orderHubClient,
            ILogger<SignalRTestController> logger)
        {
            _productHubContext = productHubContext;
            _orderHubContext = orderHubContext;
            _inventoryHubContext = inventoryHubContext;
            _productHubClient = productHubClient;
            _inventoryHubClient = inventoryHubClient;
            _orderHubClient = orderHubClient;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult TestSignalR()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetConnectionStatus()
        {
            var status = new
            {
                ProductHub = new
                {
                    IsConnected = _productHubClient.IsConnected,
                    State = _productHubClient.IsConnected ? "Connected" : "Disconnected"
                },
                InventoryHub = new
                {
                    IsConnected = _inventoryHubClient.IsConnected,
                    State = _inventoryHubClient.IsConnected ? "Connected" : "Disconnected"
                },
                OrderHub = new
                {
                    IsConnected = _orderHubClient.IsConnected,
                    State = _orderHubClient.IsConnected ? "Connected" : "Disconnected"
                },
                Timestamp = DateTime.UtcNow
            };

            return Json(status);
        }

        [HttpPost]
        public async Task<IActionResult> SendTestProductCreated()
        {
            var productId = new Random().Next(1000, 9999);
            var productName = $"Test Product {productId}";

            _logger.LogInformation("Sending test ProductCreated event: {ProductId} - {ProductName}", productId, productName);

            try
            {
                await _productHubContext.Clients.All.SendAsync("ProductCreated", productId, productName);
                return Json(new { success = true, productId, productName, message = "ProductCreated event sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ProductCreated test event");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendTestProductUpdated()
        {
            var productId = new Random().Next(1000, 9999);
            var productName = $"Updated Test Product {productId}";

            _logger.LogInformation("Sending test ProductUpdated event: {ProductId} - {ProductName}", productId, productName);

            try
            {
                await _productHubContext.Clients.All.SendAsync("ProductUpdated", productId, productName);
                return Json(new { success = true, productId, productName, message = "ProductUpdated event sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ProductUpdated test event");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendTestOrderCreated()
        {
            var orderId = new Random().Next(1000, 9999);
            var customerName = $"Test Customer {orderId}";

            _logger.LogInformation("Sending test OrderCreated event: {OrderId} - {CustomerName}", orderId, customerName);

            try
            {
                await _orderHubContext.Clients.All.SendAsync("OrderCreated", orderId, customerName);
                return Json(new { success = true, orderId, customerName, message = "OrderCreated event sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OrderCreated test event");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendTestOrderStatusChanged()
        {
            var orderId = new Random().Next(1000, 9999);
            var status = "Processing";

            _logger.LogInformation("Sending test OrderStatusChanged event: {OrderId} - {Status}", orderId, status);

            try
            {
                await _orderHubContext.Clients.All.SendAsync("OrderStatusChanged", orderId, status);
                return Json(new { success = true, orderId, status, message = "OrderStatusChanged event sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OrderStatusChanged test event");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendTestInventoryUpdated()
        {
            var inventoryId = new Random().Next(1000, 9999);
            var productId = new Random().Next(1, 100);
            var quantity = new Random().Next(0, 50);

            _logger.LogInformation("Sending test InventoryUpdated event: {InventoryId} - Product {ProductId} - Quantity {Quantity}",
                inventoryId, productId, quantity);

            try
            {
                await _inventoryHubContext.Clients.All.SendAsync("InventoryUpdated", inventoryId, productId, quantity);
                return Json(new { success = true, inventoryId, productId, quantity, message = "InventoryUpdated event sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending InventoryUpdated test event");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendTestLowStockAlert()
        {
            var inventoryId = new Random().Next(1000, 9999);
            var productId = new Random().Next(1, 100);
            var locationId = new Random().Next(1, 10);
            var quantity = new Random().Next(1, 5);
            var threshold = 10;

            _logger.LogInformation("Sending test LowStockAlert event: {InventoryId} - Product {ProductId} - Location {LocationId} - Quantity {Quantity}/{Threshold}",
                inventoryId, productId, locationId, quantity, threshold);

            try
            {
                await _inventoryHubContext.Clients.All.SendAsync("LowStockAlert", inventoryId, productId, locationId, quantity, threshold);
                return Json(new { success = true, inventoryId, productId, locationId, quantity, threshold, message = "LowStockAlert event sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending LowStockAlert test event");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendAllTestEvents()
        {
            var results = new List<object>();

            try
            {
                // Test ProductCreated
                var productResult = await SendTestProductCreated();
                results.Add(new { Event = "ProductCreated", Result = productResult });

                // Test OrderCreated
                var orderResult = await SendTestOrderCreated();
                results.Add(new { Event = "OrderCreated", Result = orderResult });

                // Test InventoryUpdated
                var inventoryResult = await SendTestInventoryUpdated();
                results.Add(new { Event = "InventoryUpdated", Result = inventoryResult });

                // Test LowStockAlert
                var lowStockResult = await SendTestLowStockAlert();
                results.Add(new { Event = "LowStockAlert", Result = lowStockResult });

                return Json(new { success = true, message = "All test events sent", results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending all test events");
                return Json(new { success = false, error = ex.Message, results });
            }
        }
    }
}
