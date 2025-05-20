using MicroservicesVisualizer.Models.Order;
using MicroservicesVisualizer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MicroservicesVisualizer.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // Order listing
        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                return View(Enumerable.Empty<OrderDto>());
            }
        }

        // Order details
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null || order.Id == 0)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order details for ID: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // Customer listing
        public async Task<IActionResult> Customers()
        {
            try
            {
                var customers = await _orderService.GetAllCustomersAsync();
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                return View(Enumerable.Empty<CustomerDto>());
            }
        }

        // Customer details
        public async Task<IActionResult> CustomerDetails(int id)
        {
            try
            {
                var customer = await _orderService.GetCustomerByIdAsync(id);

                if (customer == null || customer.Id == 0)
                {
                    return NotFound();
                }

                // Get customer orders
                var orders = await _orderService.GetOrdersByCustomerIdAsync(id);
                ViewBag.Orders = orders;

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer details for ID: {Id}", id);
                return RedirectToAction(nameof(Customers));
            }
        }

        // Filter orders by status
        public async Task<IActionResult> ByStatus(OrderStatus status)
        {
            try
            {
                var orders = await _orderService.GetOrdersByStatusAsync(status);
                ViewBag.CurrentStatus = status;
                return View("Index", orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by status: {Status}", status);
                return RedirectToAction(nameof(Index));
            }
        }

        // Order statistics
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var statistics = await _orderService.GetOrderStatisticsAsync();
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order statistics");
                return View(new OrderStatisticsDto());
            }
        }

        // Update order status form
        public async Task<IActionResult> UpdateStatus(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null || order.Id == 0)
                {
                    return NotFound();
                }

                ViewBag.Order = order;
                var model = new UpdateOrderStatusDto { Status = order.Status };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading update status form for order ID: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // Update order status submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusDto model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _orderService.UpdateOrderStatusAsync(id, model);
                    return RedirectToAction(nameof(Details), new { id });
                }

                var order = await _orderService.GetOrderByIdAsync(id);
                ViewBag.Order = order;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for ID: {Id}", id);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // Cancel order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _orderService.CancelOrderAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order with ID: {Id}", id);
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
