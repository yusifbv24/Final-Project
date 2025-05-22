using InventoryManagement.Web.Models.Inventory;
using InventoryManagement.Web.Services.ApiClients;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Web.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ILogger<InventoryController> _logger;
        private readonly InventoryApiClient _inventoryApiClient;
        private readonly ProductApiClient _productApiClient;
        private readonly LocationApiClient _locationApiClient;

        public InventoryController(
            ILogger<InventoryController> logger,
            InventoryApiClient inventoryApiClient,
            ProductApiClient productApiClient,
            LocationApiClient locationApiClient)
        {
            _logger = logger;
            _inventoryApiClient = inventoryApiClient;
            _productApiClient = productApiClient;
            _locationApiClient = locationApiClient;
        }

        public async Task<IActionResult> Index()
        {
            var inventories = await _inventoryApiClient.GetAllInventoryAsync();
            return View(inventories);
        }

        public async Task<IActionResult> Details(int id)
        {
            var inventory = await _inventoryApiClient.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            // Get transactions for this inventory item
            var transactions = await _inventoryApiClient.GetInventoryTransactionsAsync(id);
            ViewBag.Transactions = transactions;

            return View(inventory);
        }

        public async Task<IActionResult> Create()
        {
            var products = await _productApiClient.GetAllProductsAsync();
            var locations = await _locationApiClient.GetAllLocationsAsync();

            ViewBag.Products = products;
            ViewBag.Locations = locations;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInventoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var inventory = new InventoryViewModel
                {
                    ProductId = model.ProductId,
                    LocationId = model.LocationId,
                    Quantity = model.Quantity,
                    IsActive = true
                };

                var createdInventory = await _inventoryApiClient.CreateInventoryAsync(inventory);
                if (createdInventory != null)
                {
                    return RedirectToAction(nameof(Details), new { id = createdInventory.Id });
                }
            }

            var products = await _productApiClient.GetAllProductsAsync();
            var locations = await _locationApiClient.GetAllLocationsAsync();

            ViewBag.Products = products;
            ViewBag.Locations = locations;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddStock(int id, int quantity, string reference, string notes)
        {
            if (quantity <= 0)
            {
                return BadRequest("Quantity must be greater than zero");
            }

            var result = await _inventoryApiClient.AddStockAsync(id, quantity, reference, notes);
            if (result)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            return BadRequest("Failed to add stock");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveStock(int id, int quantity, string reference, string notes)
        {
            if (quantity <= 0)
            {
                return BadRequest("Quantity must be greater than zero");
            }

            var result = await _inventoryApiClient.RemoveStockAsync(id, quantity, reference, notes);
            if (result)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            return BadRequest("Failed to remove stock");
        }
    }
}
