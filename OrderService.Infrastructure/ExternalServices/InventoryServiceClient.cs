using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;

namespace OrderService.Infrastructure.ExternalServices
{
    public class InventoryServiceClient : IInventoryServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventoryServiceClient> _logger;

        public InventoryServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<InventoryServiceClient> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(configuration["Services:InventoryService"] ?? "http://localhost:5105/");
            _logger = logger;
        }

        public async Task<bool> CheckStockAvailabilityAsync(int productId, int quantity, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get inventory items for the product
                var response = await _httpClient.GetAsync($"api/v1/inventory/by-product/{productId}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return false;

                var inventoryItems = await response.Content.ReadFromJsonAsync<List<InventoryItemDto>>(cancellationToken);

                // Check if total stock across all locations is sufficient
                var totalStock = inventoryItems?.Sum(i => i.Quantity) ?? 0;
                return totalStock >= quantity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock availability: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> ReserveStockAsync(int productId, int quantity, string reference, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get inventory items for the product
                var response = await _httpClient.GetAsync($"api/v1/inventory/by-product/{productId}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return false;

                var inventoryItems = await response.Content.ReadFromJsonAsync<List<InventoryItemDto>>(cancellationToken);

                if (inventoryItems == null || !inventoryItems.Any())
                    return false;

                // Select the inventory item with the most stock
                var inventoryItem = inventoryItems.OrderByDescending(i => i.Quantity).First();

                // Check if it has enough stock
                if (inventoryItem.Quantity < quantity)
                    return false;

                // Remove stock
                var removeStockRequest = new RemoveStockRequest
                {
                    Quantity = quantity,
                    Reference = reference,
                    Notes = $"Stock reserved for order reference: {reference}"
                };

                var removeResponse = await _httpClient.PostAsJsonAsync(
                    $"api/v1/inventory/{inventoryItem.Id}/remove-stock",
                    removeStockRequest,
                    cancellationToken);

                return removeResponse.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving stock: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> ReleaseStockReservationAsync(int productId, int quantity, string reference, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get inventory items for the product
                var response = await _httpClient.GetAsync($"api/v1/inventory/by-product/{productId}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return false;

                var inventoryItems = await response.Content.ReadFromJsonAsync<List<InventoryItemDto>>(cancellationToken);

                if (inventoryItems == null || !inventoryItems.Any())
                    return false;

                // Select any inventory item (stock will be returned to the first available location)
                var inventoryItem = inventoryItems.First();

                // Add stock back
                var addStockRequest = new AddStockRequest
                {
                    Quantity = quantity,
                    Reference = reference,
                    Notes = $"Stock reservation released for order reference: {reference}"
                };

                var addResponse = await _httpClient.PostAsJsonAsync(
                    $"api/v1/inventory/{inventoryItem.Id}/add-stock",
                    addStockRequest,
                    cancellationToken);

                return addResponse.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing stock reservation: {Message}", ex.Message);
                return false;
            }
        }

        // Classes to deserialize inventory information
        private record InventoryItemDto
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int LocationId { get; set; }
            public int Quantity { get; set; }
        }

        private record AddStockRequest
        {
            public int Quantity { get; set; }
            public string Reference { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
        }

        private record RemoveStockRequest
        {
            public int Quantity { get; set; }
            public string Reference { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
        }
    }
}
