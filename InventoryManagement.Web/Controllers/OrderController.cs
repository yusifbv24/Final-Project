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
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(OrderViewModel order)
        {
            try
            {
                // Remove items with zero quantity or invalid data
                order.Items = order.Items?.Where(item =>
                    item.Quantity > 0 &&
                    item.ProductId > 0 &&
                    !string.IsNullOrEmpty(item.ProductName)).ToList() ?? new List<OrderItemViewModel>();

                // Validate that we have at least one item
                if (!order.Items.Any())
                {
                    ModelState.AddModelError("Items", "Order must contain at least one item.");
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(order.CustomerName))
                {
                    ModelState.AddModelError("CustomerName", "Customer name is required.");
                }

                if (string.IsNullOrWhiteSpace(order.CustomerEmail))
                {
                    ModelState.AddModelError("CustomerEmail", "Customer email is required.");
                }

                if (string.IsNullOrWhiteSpace(order.ShippingAddress))
                {
                    ModelState.AddModelError("ShippingAddress", "Shipping address is required.");
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Creating order for customer: {CustomerName}", order.CustomerName);

                    var createdOrder = await _orderApiClient.CreateOrderAsync(order);
                    if (createdOrder != null)
                    {
                        _logger.LogInformation("Order created successfully with ID: {OrderId}", createdOrder.Id);
                        TempData["Success"] = "Order created successfully!";
                        return RedirectToAction(nameof(Details), new { id = createdOrder.Id });
                    }
                    else
                    {
                        _logger.LogError("Failed to create order - API returned null");
                        ModelState.AddModelError("", "Failed to create order. Please try again.");
                    }
                }
                else
                {
                    _logger.LogWarning("Model validation failed for order creation");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning("Validation error: {Error}", error.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order");
                ModelState.AddModelError("", "An error occurred while creating the order. Please try again.");
            }

            // If we got here, something failed, reload the form
            var products = await _productApiClient.GetAllProductsAsync();
            ViewBag.Products = products;
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            try
            {
                var result = await _orderApiClient.UpdateOrderStatusAsync(id, status);
                if (result)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Status updated successfully" });
                    }
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to update status" });
                }

                TempData["Error"] = "Failed to update order status";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while updating status" });
                }

                TempData["Error"] = "An error occurred while updating order status";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
