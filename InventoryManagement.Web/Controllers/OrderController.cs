using InventoryManagement.Web.Models.Order;
using InventoryManagement.Web.Services.ApiClients;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly ILogger<OrderController> _logger;
        private readonly OrderApiClient _orderApiClient;
        private readonly ProductApiClient _productApiClient;

        public OrderController(
            ILogger<OrderController> logger,
            OrderApiClient orderApiClient,
            ProductApiClient productApiClient)
        {
            _logger = logger;
            _orderApiClient = orderApiClient;
            _productApiClient = productApiClient;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderApiClient.GetAllOrdersAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderApiClient.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        public async Task<IActionResult> Create()
        {
            var products = await _productApiClient.GetAllProductsAsync();
            ViewBag.Products = products;
            return View(new OrderViewModel
            {
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderViewModel order)
        {
            if (ModelState.IsValid)
            {
                var createdOrder = await _orderApiClient.CreateOrderAsync(order);
                if (createdOrder != null)
                {
                    return RedirectToAction(nameof(Details), new { id = createdOrder.Id });
                }
            }

            var products = await _productApiClient.GetAllProductsAsync();
            ViewBag.Products = products;
            return View(order);
        }

        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var result = await _orderApiClient.UpdateOrderStatusAsync(id, status);
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
