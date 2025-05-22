using InventoryManagement.Web.Models.Product;

namespace InventoryManagement.Web.Services.ApiClients
{
    public class CategoryApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CategoryApiClient> _logger;

        public CategoryApiClient(HttpClient httpClient, ILogger<CategoryApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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

        public async Task<CategoryViewModel?> GetCategoryByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<CategoryViewModel>($"api/v1/categories/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category with ID {CategoryId}", id);
                return null;
            }
        }

        public async Task<CategoryViewModel?> CreateCategoryAsync(CategoryViewModel category)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/categories", category);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CategoryViewModel>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return null;
            }
        }

        public async Task<CategoryViewModel?> UpdateCategoryAsync(int id, CategoryViewModel category)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/v1/categories/{id}", category);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CategoryViewModel>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return null;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/v1/categories/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return false;
            }
        }
    }
}