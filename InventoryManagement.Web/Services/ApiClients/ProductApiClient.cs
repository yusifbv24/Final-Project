using InventoryManagement.Web.Models.Product;

namespace InventoryManagement.Web.Services.ApiClients
{
    public class ProductApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductApiClient> _logger;

        public ProductApiClient(HttpClient httpClient, ILogger<ProductApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ProductViewModel>> GetAllProductsAsync()
        {
            try
            {
                var products = await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("api/v1/products");
                return products ?? new List<ProductViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return new List<ProductViewModel>();
            }
        }

        public async Task<ProductViewModel?> GetProductByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ProductViewModel>($"api/v1/products/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product with ID {ProductId}", id);
                return null;
            }
        }

        public async Task<List<ProductViewModel>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                var products = await _httpClient.GetFromJsonAsync<List<ProductViewModel>>($"api/v1/products/by-category/{categoryId}");
                return products ?? new List<ProductViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category ID {CategoryId}", categoryId);
                return new List<ProductViewModel>();
            }
        }

        public async Task<ProductViewModel?> CreateProductAsync(ProductViewModel product)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/products", product);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ProductViewModel>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return null;
            }
        }

        public async Task<ProductViewModel?> UpdateProductAsync(int id, ProductViewModel product)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/v1/products/{id}", product);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ProductViewModel>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return null;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/v1/products/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return false;
            }
        }
    }
}
