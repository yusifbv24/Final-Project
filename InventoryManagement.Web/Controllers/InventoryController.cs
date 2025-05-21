using InventoryManagement.Web.Services.ApiClients;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Web.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ILogger<InventoryController> _logger;
        private readonly InventoryApiClient _inventoryApiClient;
        private readonly ProductApiClient _productApiClient;

        public InventoryController(
            ILogger<InventoryController> logger,
            InventoryApiClient inventoryApiClient,
            ProductApiClient productApiClient)
        {
            _logger = logger;
            _inventoryApiClient = inventoryApiClient;
            _productApiClient = productApiClient;
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
            return View(inventory);
        }
    }
}
