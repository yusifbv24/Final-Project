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
                var response = await _httpClient.GetAsync($"api/v1/inventory/by-product/{productId}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return false;

                var inventories = await response.Content.ReadFromJsonAsync<List<InventoryDto>>(cancellationToken);
                if (inventories == null || !inventories.Any())
                    return false;

                // Sum available quantity across all locations
                var totalAvailableQuantity = inventories.Sum(i => i.Quantity);
                return totalAvailableQuantity >= quantity;
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
                // First, get inventory locations for this product
                var response = await _httpClient.GetAsync($"api/v1/inventory/by-product/{productId}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return false;

                var inventories = await response.Content.ReadFromJsonAsync<List<InventoryDto>>(cancellationToken);
                if (inventories == null || !inventories.Any())
                    return false;

                // Find first location with enough stock
                var inventory = inventories.FirstOrDefault(i => i.Quantity >= quantity);
                if (inventory == null)
                    return false;

                // Reserve stock by removing it
                var requestBody = new RemoveStockRequest(quantity, reference, "Reserved for order");
                var removeResponse = await _httpClient.PostAsJsonAsync(
                    $"api/v1/inventory/{inventory.Id}/remove-stock",
                    requestBody,
                    cancellationToken);

                return removeResponse.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving stock: {Message}", ex.Message);
                return false;
            }
        }

        // DTOs for serialization
        private record InventoryDto
        {
            public int Id { get; init; }
            public int ProductId { get; init; }
            public int LocationId { get; init; }
            public int Quantity { get; init; }
        }

        private record RemoveStockRequest(int Quantity, string Reference, string Notes);
    }
}
