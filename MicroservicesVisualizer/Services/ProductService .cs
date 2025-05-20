using MicroservicesVisualizer.Services.Interfaces;
using System.Text.Json;
using System.Text;
using MicroservicesVisualizer.Models.Product;

namespace MicroservicesVisualizer.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Product methods
        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v1/products");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>(_jsonOptions)
                    ?? Enumerable.Empty<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                return Enumerable.Empty<ProductDto>();
            }
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/products/{id}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ProductDto>(_jsonOptions)
                    ?? new ProductDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(int categoryId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/products/by-category/{categoryId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>(_jsonOptions)
                    ?? Enumerable.Empty<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for category ID: {CategoryId}", categoryId);
                return Enumerable.Empty<ProductDto>();
            }
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string? keyword, decimal? minPrice, decimal? maxPrice)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(keyword)) queryParams.Add($"keyword={Uri.EscapeDataString(keyword)}");
                if (minPrice.HasValue) queryParams.Add($"minPrice={minPrice}");
                if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

                var response = await _httpClient.GetAsync($"api/v2/products/search{queryString}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>(_jsonOptions)
                    ?? Enumerable.Empty<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products");
                return Enumerable.Empty<ProductDto>();
            }
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/v1/products", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ProductDto>(_jsonOptions)
                    ?? new ProductDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/v1/products/{id}", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ProductDto>(_jsonOptions)
                    ?? new ProductDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/v1/products/{id}");
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {Id}", id);
                return false;
            }
        }

        // Category methods
        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v1/categories");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<CategoryDto>>(_jsonOptions)
                    ?? Enumerable.Empty<CategoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all categories");
                return Enumerable.Empty<CategoryDto>();
            }
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/categories/{id}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<CategoryDto>(_jsonOptions)
                    ?? new CategoryDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesWithProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v2/categories/with-products");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<CategoryDto>>(_jsonOptions)
                    ?? Enumerable.Empty<CategoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories with products");
                return Enumerable.Empty<CategoryDto>();
            }
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/v1/categories", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<CategoryDto>(_jsonOptions)
                    ?? new CategoryDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                throw;
            }
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/v1/categories/{id}", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<CategoryDto>(_jsonOptions)
                    ?? new CategoryDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/v1/categories/{id}");
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID: {Id}", id);
                return false;
            }
        }
    }
}
