using System.Net.Http.Json;
using InventoryService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InventoryService.Infrastructure.ExternalServices
{
    public class ProductServiceClient : IProductServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductServiceClient> _logger;

        public ProductServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<ProductServiceClient> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(configuration["Services:ProductService"] ?? "http://localhost:5104/");
            _logger = logger;
        }

        public async Task<bool> ProductExistsAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/products/{productId}", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product exists: {Message}", ex.Message);
                // In case of error, we assume the product doesn't exist
                return false;
            }
        }

        public async Task<string> GetProductNameAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _httpClient.GetFromJsonAsync<ProductDto>($"api/v1/products/{productId}", cancellationToken);
                return product?.Name ?? $"Product {productId}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product name: {Message}", ex.Message);
                return $"Product {productId}";
            }
        }

        // Class to deserialize product information
        private record ProductDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
