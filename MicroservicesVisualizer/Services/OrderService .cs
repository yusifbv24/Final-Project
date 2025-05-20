using MicroservicesVisualizer.Services.Interfaces;
using System.Text.Json;
using System.Text;
using MicroservicesVisualizer.Models.Order;

namespace MicroservicesVisualizer.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public OrderService(HttpClient httpClient, ILogger<OrderService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Order methods
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v1/orders");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<OrderDto>>(_jsonOptions)
                    ?? Enumerable.Empty<OrderDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders");
                return Enumerable.Empty<OrderDto>();
            }
        }

        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/orders/{id}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions)
                    ?? new OrderDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/orders/by-customer/{customerId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<OrderDto>>(_jsonOptions)
                    ?? Enumerable.Empty<OrderDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for customer ID: {CustomerId}", customerId);
                return Enumerable.Empty<OrderDto>();
            }
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/orders/by-status/{status}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<OrderDto>>(_jsonOptions)
                    ?? Enumerable.Empty<OrderDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders with status: {Status}", status);
                return Enumerable.Empty<OrderDto>();
            }
        }

        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v2/orders/stats");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<OrderStatisticsDto>(_jsonOptions)
                    ?? new OrderStatisticsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order statistics");
                return new OrderStatisticsDto();
            }
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/v1/orders", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions)
                    ?? new OrderDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                throw;
            }
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/v1/orders/{id}/status", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions)
                    ?? new OrderDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for ID: {Id}", id);
                throw;
            }
        }

        public async Task<OrderDto> UpdateOrderAddressAsync(int id, UpdateOrderAddressDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/v1/orders/{id}/address", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions)
                    ?? new OrderDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order address for ID: {Id}", id);
                throw;
            }
        }

        public async Task<OrderDto> CancelOrderAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/v1/orders/{id}/cancel", null);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions)
                    ?? new OrderDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order with ID: {Id}", id);
                throw;
            }
        }

        // Customer methods
        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v1/customers");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<CustomerDto>>(_jsonOptions)
                    ?? Enumerable.Empty<CustomerDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all customers");
                return Enumerable.Empty<CustomerDto>();
            }
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/customers/{id}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions)
                    ?? new CustomerDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer with ID: {Id}", id);
                throw;
            }
        }

        public async Task<CustomerDto> GetCustomerByEmailAsync(string email)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/customers/by-email/{email}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions)
                    ?? new CustomerDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer with email: {Email}", email);
                throw;
            }
        }

        public async Task<CustomerDto> CreateCustomerAsync(CustomerDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/v1/customers", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions)
                    ?? new CustomerDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                throw;
            }
        }

        public async Task<CustomerDto> UpdateCustomerAsync(int id, CustomerDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/v1/customers/{id}", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions)
                    ?? new CustomerDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer with ID: {Id}", id);
                throw;
            }
        }

        public async Task<CustomerDto> ActivateCustomerAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/v1/customers/{id}/activate", null);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions)
                    ?? new CustomerDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating customer with ID: {Id}", id);
                throw;
            }
        }

        public async Task<CustomerDto> DeactivateCustomerAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/v1/customers/{id}/deactivate", null);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions)
                    ?? new CustomerDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating customer with ID: {Id}", id);
                throw;
            }
        }
    }
}
