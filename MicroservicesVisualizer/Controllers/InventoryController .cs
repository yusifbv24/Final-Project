using MicroservicesVisualizer.Models.Inventory;
using MicroservicesVisualizer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MicroservicesVisualizer.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IProductService _productService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            IInventoryService inventoryService,
            IProductService productService,
            ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _productService = productService;
            _logger = logger;
        }

        // Inventory listing
        public async Task<IActionResult> Index()
        {
            try
            {
                var inventories = await _inventoryService.GetAllInventoryAsync();
                return View(inventories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory items");
                return View(Enumerable.Empty<InventoryDto>());
            }
        }

        // Inventory details
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(id);

                if (inventory == null || inventory.Id == 0)
                {
                    return NotFound();
                }

                // Get transactions for this inventory
                var transactions = await _inventoryService.GetTransactionsByInventoryIdAsync(id);
                ViewBag.Transactions = transactions;

                return View(inventory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory details for ID: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // Add stock form
        public async Task<IActionResult> AddStock(int id)
        {
            try
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(id);

                if (inventory == null || inventory.Id == 0)
                {
                    return NotFound();
                }

                ViewBag.Inventory = inventory;
                return View(new AddStockRequest());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading add stock form for inventory ID: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // Add stock submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStock(int id, AddStockRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _inventoryService.AddStockAsync(id, request);
                    return RedirectToAction(nameof(Details), new { id });
                }

                var inventory = await _inventoryService.GetInventoryByIdAsync(id);
                ViewBag.Inventory = inventory;
                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding stock to inventory ID: {Id}", id);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // Remove stock form
        public async Task<IActionResult> RemoveStock(int id)
        {
            try
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(id);

                if (inventory == null || inventory.Id == 0)
                {
                    return NotFound();
                }

                ViewBag.Inventory = inventory;
                return View(new RemoveStockRequest());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading remove stock form for inventory ID: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // Remove stock submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStock(int id, RemoveStockRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _inventoryService.RemoveStockAsync(id, request);
                    return RedirectToAction(nameof(Details), new { id });
                }

                var inventory = await _inventoryService.GetInventoryByIdAsync(id);
                ViewBag.Inventory = inventory;
                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing stock from inventory ID: {Id}", id);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // Location listing
        public async Task<IActionResult> Locations()
        {
            try
            {
                var locations = await _inventoryService.GetLocationsWithInventoryAsync();
                return View(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving locations");
                return View(Enumerable.Empty<LocationWithInventoryDto>());
            }
        }

        // Transaction listing
        public async Task<IActionResult> Transactions()
        {
            try
            {
                var transactions = await _inventoryService.GetAllTransactionsAsync();
                return View(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions");
                return View(Enumerable.Empty<InventoryTransactionDto>());
            }
        }

        // Search inventory
        public async Task<IActionResult> Search(int? productId, int? locationId, int? minQuantity, int? maxQuantity, bool? isActive)
        {
            try
            {
                var inventories = await _inventoryService.SearchInventoryAsync(productId, locationId, minQuantity, maxQuantity, isActive);
                ViewBag.ProductId = productId;
                ViewBag.LocationId = locationId;
                ViewBag.MinQuantity = minQuantity;
                ViewBag.MaxQuantity = maxQuantity;
                ViewBag.IsActive = isActive;

                // Get products and locations for the filter dropdowns
                var products = await _productService.GetAllProductsAsync();
                var locations = await _inventoryService.GetAllLocationsAsync();
                ViewBag.Products = products;
                ViewBag.Locations = locations;

                return View("Index", inventories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching inventory");
                return RedirectToAction(nameof(Index));
            }
        }

        // Low stock items
        public async Task<IActionResult> LowStock(int threshold = 10)
        {
            try
            {
                var lowStockItems = await _inventoryService.GetLowStockItemsAsync(threshold);
                ViewBag.Threshold = threshold;
                return View(lowStockItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low stock items");
                return View(Enumerable.Empty<LowStockItemDto>());
            }
        }
    }
}
