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

        public async Task<List<InventoryViewModel>> GetInventoryByProductAsync(int productId)
        {
            try
            {
                var inventories = await _httpClient.GetFromJsonAsync<List<InventoryViewModel>>($"api/v1/inventory/by-product/{productId}");
                return inventories ?? new List<InventoryViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory by product ID {ProductId}", productId);
                return new List<InventoryViewModel>();
            }
        }

        public async Task<List<InventoryViewModel>> GetInventoryByLocationAsync(int locationId)
        {
            try
            {
                var inventories = await _httpClient.GetFromJsonAsync<List<InventoryViewModel>>($"api/v1/inventory/by-location/{locationId}");
                return inventories ?? new List<InventoryViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory by location ID {LocationId}", locationId);
                return new List<InventoryViewModel>();
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

        public async Task<InventoryViewModel?> CreateInventoryAsync(InventoryViewModel inventory)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/inventory", inventory);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<InventoryViewModel>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory");
                return null;
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

        public async Task<bool> AddStockAsync(int id, int quantity, string reference, string notes)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/v1/inventory/{id}/add-stock",
                    new { Quantity = quantity, Reference = reference, Notes = notes });
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding stock to inventory");
                return false;
            }
        }

        public async Task<bool> RemoveStockAsync(int id, int quantity, string reference, string notes)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/v1/inventory/{id}/remove-stock",
                    new { Quantity = quantity, Reference = reference, Notes = notes });
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing stock from inventory");
                return false;
            }
        }

        public async Task<List<InventoryTransactionViewModel>> GetInventoryTransactionsAsync(int inventoryId)
        {
            try
            {
                var transactions = await _httpClient.GetFromJsonAsync<List<InventoryTransactionViewModel>>($"api/v1/inventorytransaction/by-inventory/{inventoryId}");
                return transactions ?? new List<InventoryTransactionViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for inventory ID {InventoryId}", inventoryId);
                return new List<InventoryTransactionViewModel>();
            }
        }
    }
}
