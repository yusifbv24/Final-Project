using InventoryManagement.Web.Services.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace InventoryManagement.Web.Controllers
{
    public class SignalRTestController : Controller
    {
        private readonly IHubContext<ProductHubProxy> _productHubContext;
        private readonly IHubContext<OrderHubProxy> _orderHubContext;
        private readonly ILogger<SignalRTestController> _logger;

        public SignalRTestController(
            IHubContext<ProductHubProxy> productHubContext,
            IHubContext<OrderHubProxy> orderHubContext,
            ILogger<SignalRTestController> logger)
        {
            _productHubContext = productHubContext;
            _orderHubContext = orderHubContext;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult TestSignalR()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendTestProductCreated()
        {
            var productId = new Random().Next(1000, 9999);
            var productName = $"Test Product {productId}";

            _logger.LogInformation("Sending test ProductCreated event: {ProductId} - {ProductName}", productId, productName);

            await _productHubContext.Clients.All.SendAsync("ProductCreated", productId, productName);

            return Json(new { success = true, productId, productName });
        }

        [HttpPost]
        public async Task<IActionResult> SendTestOrderCreated()
        {
            var orderId = new Random().Next(1000, 9999);
            var customerName = $"Test Customer {orderId}";

            _logger.LogInformation("Sending test OrderCreated event: {OrderId} - {CustomerName}", orderId, customerName);

            await _orderHubContext.Clients.All.SendAsync("OrderCreated", orderId, customerName);

            return Json(new { success = true, orderId, customerName });
        }
    }
}
