using InventoryManagement.Web.Models.Inventory;
using InventoryManagement.Web.Services.ApiClients;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Web.Controllers
{
    public class LocationController : Controller
    {
        private readonly ILogger<LocationController> _logger;
        private readonly LocationApiClient _locationApiClient;
        private readonly InventoryApiClient _inventoryApiClient;

        public LocationController(
            ILogger<LocationController> logger,
            LocationApiClient locationApiClient,
            InventoryApiClient inventoryApiClient)
        {
            _logger = logger;
            _locationApiClient = locationApiClient;
            _inventoryApiClient = inventoryApiClient;
        }

        public async Task<IActionResult> Index()
        {
            var locations = await _locationApiClient.GetAllLocationsAsync();
            return View(locations);
        }

        public async Task<IActionResult> Details(int id)
        {
            var location = await _locationApiClient.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            // Get inventory items at this location
            var inventories = await _inventoryApiClient.GetInventoryByLocationAsync(id);
            ViewBag.Inventories = inventories;

            return View(location);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLocationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var location = new LocationViewModel
                {
                    Name = model.Name,
                    Code = model.Code,
                    Description = model.Description,
                    IsActive = true
                };

                var createdLocation = await _locationApiClient.CreateLocationAsync(location);
                if (createdLocation != null)
                {
                    return RedirectToAction(nameof(Details), new { id = createdLocation.Id });
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var location = await _locationApiClient.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            var model = new EditLocationViewModel
            {
                Id = location.Id,
                Name = location.Name,
                Code = location.Code,
                Description = location.Description,
                IsActive = location.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditLocationViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var location = new LocationViewModel
                {
                    Id = model.Id,
                    Name = model.Name,
                    Code = model.Code,
                    Description = model.Description,
                    IsActive = model.IsActive
                };

                var updatedLocation = await _locationApiClient.UpdateLocationAsync(id, location);
                if (updatedLocation != null)
                {
                    return RedirectToAction(nameof(Details), new { id = updatedLocation.Id });
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var location = await _locationApiClient.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _locationApiClient.DeleteLocationAsync(id);
            if (success)
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Delete), new { id = id });
        }
    }
}
