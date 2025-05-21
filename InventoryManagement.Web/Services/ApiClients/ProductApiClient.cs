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

        public async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("api/v1/categories");
                return categories ?? new List<CategoryViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return new List<CategoryViewModel>();
            }
        }
    }
}
