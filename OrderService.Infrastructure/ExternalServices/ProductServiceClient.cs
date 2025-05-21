using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;

namespace OrderService.Infrastructure.ExternalServices
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

        public async Task<ProductInfo?> GetProductAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _httpClient.GetFromJsonAsync<ProductDto>($"api/v1/products/{productId}", cancellationToken);
                if (product == null)
                    return null;

                return new ProductInfo(
                    product.Id,
                    product.Name,
                    product.Price,
                    product.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product: {Message}", ex.Message);
                return null;
            }
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
                return false;
            }
        }

        // DTO to deserialize product information
        private record ProductDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
