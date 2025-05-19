using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplierService.Application.Interfaces;

namespace SupplierService.Infrastructure.ExternalServices
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

        public async Task<bool> AddInventoryStockAsync(int productId, int quantity, string reference, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get inventory item for the product
                var response = await _httpClient.GetAsync($"api/v1/inventory/by-product/{productId}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return false;

                var inventoryItems = await response.Content.ReadFromJsonAsync<List<InventoryItemDto>>(cancellationToken);

                if (inventoryItems == null || !inventoryItems.Any())
                    return false;

                // Select the inventory item with the most room for stock
                var inventoryItem = inventoryItems.First();

                // Add stock
                var addStockRequest = new AddStockRequest
                {
                    Quantity = quantity,
                    Reference = reference,
                    Notes = $"Stock added from purchase order: {reference}"
                };

                var addResponse = await _httpClient.PostAsJsonAsync(
                    $"api/v1/inventory/{inventoryItem.Id}/add-stock",
                    addStockRequest,
                    cancellationToken);

                return addResponse.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding inventory stock: {Message}", ex.Message);
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
    }
}
