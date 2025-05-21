using InventoryManagement.Web.Models.Inventory;

namespace InventoryManagement.Web.Services.ApiClients
{
    public class InventoryApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventoryApiClient> _logger;

        public InventoryApiClient(HttpClient httpClient, ILogger<InventoryApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<InventoryViewModel>> GetAllInventoryAsync()
        {
            try
            {
                var inventories = await _httpClient.GetFromJsonAsync<List<InventoryViewModel>>("api/v1/inventory");
                return inventories ?? new List<InventoryViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all inventory items");
                return new List<InventoryViewModel>();
            }
        }

        public async Task<InventoryViewModel?> GetInventoryByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<InventoryViewModel>($"api/v1/inventory/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory with ID {InventoryId}", id);
                return null;
            }
        }

        public async Task<List<LocationViewModel>> GetAllLocationsAsync()
        {
            try
            {
                var locations = await _httpClient.GetFromJsonAsync<List<LocationViewModel>>("api/v1/location");
                return locations ?? new List<LocationViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all locations");
                return new List<LocationViewModel>();
            }
        }

        public async Task<bool> UpdateInventoryQuantityAsync(int id, int quantity)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/v1/inventory/{id}/quantity", new { Quantity = quantity });
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory quantity");
                return false;
            }
        }
    }
}
