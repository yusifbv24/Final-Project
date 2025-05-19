using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplierService.Application.Interfaces;

namespace SupplierService.Infrastructure.ExternalServices
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
                return false;
            }
        }

        public async Task<ProductInfo?> GetProductAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _httpClient.GetFromJsonAsync<ProductDetailDto>(
                    $"api/v1/products/{productId}", cancellationToken);

                return product != null
                    ? new ProductInfo(product.Id, product.Name, product.Price)
                    : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product details: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<IEnumerable<ProductInfo>> GetProductsAsync(IEnumerable<int> productIds, CancellationToken cancellationToken = default)
        {
            var result = new List<ProductInfo>();

            foreach (var productId in productIds)
            {
                try
                {
                    var product = await GetProductAsync(productId, cancellationToken);
                    if (product != null)
                        result.Add(product);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting product {ProductId}: {Message}", productId, ex.Message);
                }
            }

            return result;
        }

        // Class to deserialize product information
        private class ProductDetailDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
        }
    }
}
