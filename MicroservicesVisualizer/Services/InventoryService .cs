using MicroservicesVisualizer.Services.Interfaces;
using System.Text.Json;
using System.Text;
using MicroservicesVisualizer.Models.Inventory;

namespace MicroservicesVisualizer.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventoryService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public InventoryService(HttpClient httpClient, ILogger<InventoryService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Inventory methods
        public async Task<IEnumerable<InventoryDto>> GetAllInventoryAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v1/inventory");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<InventoryDto>>(_jsonOptions)
                    ?? Enumerable.Empty<InventoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all inventory items");
                return Enumerable.Empty<InventoryDto>();
            }
        }

        public async Task<InventoryDto> GetInventoryByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/inventory/{id}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<InventoryDto>(_jsonOptions)
                    ?? new InventoryDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory item with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<InventoryDto>> GetInventoryByProductIdAsync(int productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/inventory/by-product/{productId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<InventoryDto>>(_jsonOptions)
                    ?? Enumerable.Empty<InventoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory items for product ID: {ProductId}", productId);
                return Enumerable.Empty<InventoryDto>();
            }
        }

        public async Task<IEnumerable<InventoryDto>> GetInventoryByLocationIdAsync(int locationId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/inventory/by-location/{locationId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<InventoryDto>>(_jsonOptions)
                    ?? Enumerable.Empty<InventoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory items for location ID: {LocationId}", locationId);
                return Enumerable.Empty<InventoryDto>();
            }
        }

        public async Task<IEnumerable<InventoryDto>> SearchInventoryAsync(int? productId, int? locationId, int? minQuantity, int? maxQuantity, bool? isActive)
        {
            try
            {
                var queryParams = new List<string>();
                if (productId.HasValue) queryParams.Add($"productId={productId}");
                if (locationId.HasValue) queryParams.Add($"locationId={locationId}");
                if (minQuantity.HasValue) queryParams.Add($"minQuantity={minQuantity}");
                if (maxQuantity.HasValue) queryParams.Add($"maxQuantity={maxQuantity}");
                if (isActive.HasValue) queryParams.Add($"isActive={isActive}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

                var response = await _httpClient.GetAsync($"api/v2/inventory/search{queryString}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<InventoryDto>>(_jsonOptions)
                    ?? Enumerable.Empty<InventoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching inventory items");
                return Enumerable.Empty<InventoryDto>();
            }
        }

        public async Task<IEnumerable<LowStockItemDto>> GetLowStockItemsAsync(int threshold = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v2/inventory/low-stock?threshold={threshold}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<LowStockItemDto>>(_jsonOptions)
                    ?? Enumerable.Empty<LowStockItemDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low stock items with threshold: {Threshold}", threshold);
                return Enumerable.Empty<LowStockItemDto>();
            }
        }

        // Location methods
        public async Task<IEnumerable<LocationDto>> GetAllLocationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v1/location");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<LocationDto>>(_jsonOptions)
                    ?? Enumerable.Empty<LocationDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all locations");
                return Enumerable.Empty<LocationDto>();
            }
        }

        public async Task<LocationDto> GetLocationByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/location/{id}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<LocationDto>(_jsonOptions)
                    ?? new LocationDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving location with ID: {Id}", id);
                throw;
            }
        }

        public async Task<LocationDto> GetLocationByCodeAsync(string code)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/location/by-code/{code}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<LocationDto>(_jsonOptions)
                    ?? new LocationDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving location with code: {Code}", code);
                throw;
            }
        }

        public async Task<IEnumerable<LocationWithInventoryDto>> GetLocationsWithInventoryAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v2/location/with-inventory");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<LocationWithInventoryDto>>(_jsonOptions)
                    ?? Enumerable.Empty<LocationWithInventoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving locations with inventory");
                return Enumerable.Empty<LocationWithInventoryDto>();
            }
        }

        // Transaction methods
        public async Task<IEnumerable<InventoryTransactionDto>> GetAllTransactionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v1/inventorytransaction");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<InventoryTransactionDto>>(_jsonOptions)
                    ?? Enumerable.Empty<InventoryTransactionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all inventory transactions");
                return Enumerable.Empty<InventoryTransactionDto>();
            }
        }

        public async Task<InventoryTransactionDto> GetTransactionByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/inventorytransaction/{id}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<InventoryTransactionDto>(_jsonOptions)
                    ?? new InventoryTransactionDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory transaction with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByInventoryIdAsync(int inventoryId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/inventorytransaction/by-inventory/{inventoryId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<InventoryTransactionDto>>(_jsonOptions)
                    ?? Enumerable.Empty<InventoryTransactionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for inventory ID: {InventoryId}", inventoryId);
                return Enumerable.Empty<InventoryTransactionDto>();
            }
        }

        public async Task<IEnumerable<InventoryTransactionDto>> SearchTransactionsAsync(int? inventoryId, TransactionType? type, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var queryParams = new List<string>();
                if (inventoryId.HasValue) queryParams.Add($"inventoryId={inventoryId}");
                if (type.HasValue) queryParams.Add($"type={type}");
                if (startDate.HasValue) queryParams.Add($"startDate={startDate:o}");
                if (endDate.HasValue) queryParams.Add($"endDate={endDate:o}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

                var response = await _httpClient.GetAsync($"api/v2/inventorytransaction/search{queryString}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<InventoryTransactionDto>>(_jsonOptions)
                    ?? Enumerable.Empty<InventoryTransactionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching inventory transactions");
                return Enumerable.Empty<InventoryTransactionDto>();
            }
        }

        public async Task<TransactionSummaryDto> GetTransactionSummaryAsync(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var queryParams = new List<string>();
                if (startDate.HasValue) queryParams.Add($"startDate={startDate:o}");
                if (endDate.HasValue) queryParams.Add($"endDate={endDate:o}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

                var response = await _httpClient.GetAsync($"api/v2/inventorytransaction/summary{queryString}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<TransactionSummaryDto>(_jsonOptions)
                    ?? new TransactionSummaryDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction summary");
                return new TransactionSummaryDto();
            }
        }

        // CRUD operations
        public async Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/v1/inventory", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<InventoryDto>(_jsonOptions)
                    ?? new InventoryDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory item");
                throw;
            }
        }

        public async Task<InventoryDto> UpdateInventoryQuantityAsync(int id, UpdateInventoryDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/v1/inventory/{id}/quantity", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<InventoryDto>(_jsonOptions)
                    ?? new InventoryDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory quantity for ID: {Id}", id);
                throw;
            }
        }

        public async Task<InventoryDto> AddStockAsync(int id, AddStockRequest request)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(request, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"api/v1/inventory/{id}/add-stock", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<InventoryDto>(_jsonOptions)
                    ?? new InventoryDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding stock to inventory ID: {Id}", id);
                throw;
            }
        }

        public async Task<InventoryDto> RemoveStockAsync(int id, RemoveStockRequest request)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(request, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"api/v1/inventory/{id}/remove-stock", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<InventoryDto>(_jsonOptions)
                    ?? new InventoryDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing stock from inventory ID: {Id}", id);
                throw;
            }
        }
    }
}
