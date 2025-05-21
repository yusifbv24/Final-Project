using System.Text.Json;
using System.Text.Json.Serialization;
using InventoryManagement.Web.Models.Order;

namespace InventoryManagement.Web.Services.ApiClients
{
    public class OrderApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() } // Parse enums as strings
        };

        public OrderApiClient(HttpClient httpClient, ILogger<OrderApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<OrderViewModel>> GetAllOrdersAsync()
        {
            var orders = await _httpClient.GetFromJsonAsync<List<OrderViewModel>>(
                "api/v1/order", _jsonOptions
            );
            return orders ?? new List<OrderViewModel>();
        }

        public async Task<OrderViewModel?> GetOrderByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<OrderViewModel>($"api/v1/order/{id}",_jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order with ID {OrderId}", id);
                return null;
            }
        }

        public async Task<OrderViewModel?> CreateOrderAsync(OrderViewModel order)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/order", order,_jsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<OrderViewModel>(_jsonOptions);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return null;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, OrderStatus status)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/v1/order/{id}/status", new { Status = status }, _jsonOptions);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return false;
            }
        }
    }
}
