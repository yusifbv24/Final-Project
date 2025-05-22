using InventoryManagement.Web.Models.Inventory;

namespace InventoryManagement.Web.Services.ApiClients
{
    public class LocationApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LocationApiClient> _logger;

        public LocationApiClient(HttpClient httpClient, ILogger<LocationApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<LocationViewModel>> GetAllLocationsAsync()
        {
            try
            {
                var locations = await _httpClient.GetFromJsonAsync<List<LocationViewModel>>("api/v1/location");
                return locations ?? new List<LocationViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all locations");
                return new List<LocationViewModel>();
            }
        }

        public async Task<LocationViewModel?> GetLocationByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<LocationViewModel>($"api/v1/location/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location with ID {LocationId}", id);
                return null;
            }
        }

        public async Task<LocationViewModel?> GetLocationByCodeAsync(string code)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<LocationViewModel>($"api/v1/location/by-code/{code}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location with code {LocationCode}", code);
                return null;
            }
        }

        public async Task<LocationViewModel?> CreateLocationAsync(LocationViewModel location)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/location", location);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<LocationViewModel>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating location");
                return null;
            }
        }

        public async Task<LocationViewModel?> UpdateLocationAsync(int id, LocationViewModel location)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/v1/location/{id}", location);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<LocationViewModel>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location");
                return null;
            }
        }

        public async Task<bool> DeleteLocationAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/v1/location/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting location");
                return false;
            }
        }
    }
}
