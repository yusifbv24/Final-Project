using MicroservicesVisualizer.Services.Interfaces;
using System.Text.Json;
using System.Text;
using MicroservicesVisualizer.Models.Supplier;

namespace MicroservicesVisualizer.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SupplierService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public SupplierService(HttpClient httpClient, ILogger<SupplierService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Supplier methods
        public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v1/suppliers");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<SupplierDto>>(_jsonOptions)
                    ?? Enumerable.Empty<SupplierDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all suppliers");
                return Enumerable.Empty<SupplierDto>();
            }
        }

        public async Task<SupplierDto> GetSupplierByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/suppliers/{id}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<SupplierDto>(_jsonOptions)
                    ?? new SupplierDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier with ID: {Id}", id);
                throw;
            }
        }

        public async Task<SupplierDto> GetSupplierByEmailAsync(string email)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/suppliers/by-email/{email}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<SupplierDto>(_jsonOptions)
                    ?? new SupplierDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier with email: {Email}", email);
                throw;
            }
        }

        public async Task<IEnumerable<SupplierDto>> SearchSuppliersAsync(string? name, string? email, bool? isActive)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(name)) queryParams.Add($"name={Uri.EscapeDataString(name)}");
                if (!string.IsNullOrEmpty(email)) queryParams.Add($"email={Uri.EscapeDataString(email)}");
                if (isActive.HasValue) queryParams.Add($"isActive={isActive}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

                var response = await _httpClient.GetAsync($"api/v2/suppliers/search{queryString}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<SupplierDto>>(_jsonOptions)
                    ?? Enumerable.Empty<SupplierDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching suppliers");
                return Enumerable.Empty<SupplierDto>();
            }
        }

        public async Task<SupplierWithOrdersDto> GetSupplierWithOrdersAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v2/suppliers/{id}/with-orders");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<SupplierWithOrdersDto>(_jsonOptions)
                    ?? new SupplierWithOrdersDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier with orders for ID: {Id}", id);
                throw;
            }
        }

        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/v1/suppliers", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<SupplierDto>(_jsonOptions)
                    ?? new SupplierDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier");
                throw;
            }
        }

        public async Task<SupplierDto> UpdateSupplierAsync(int id, UpdateSupplierDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/v1/suppliers/{id}", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<SupplierDto>(_jsonOptions)
                    ?? new SupplierDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier with ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/v1/suppliers/{id}");
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier with ID: {Id}", id);
                return false;
            }
        }

        public async Task<SupplierDto> ActivateSupplierAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/v1/suppliers/{id}/activate", null);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<SupplierDto>(_jsonOptions)
                    ?? new SupplierDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating supplier with ID: {Id}", id);
                throw;
            }
        }

        public async Task<SupplierDto> DeactivateSupplierAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/v1/suppliers/{id}/deactivate", null);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<SupplierDto>(_jsonOptions)
                    ?? new SupplierDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating supplier with ID: {Id}", id);
                throw;
            }
        }

        // Purchase Order methods
        public async Task<IEnumerable<PurchaseOrderDto>> GetAllPurchaseOrdersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v1/purchaseorders");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<PurchaseOrderDto>>(_jsonOptions)
                    ?? Enumerable.Empty<PurchaseOrderDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all purchase orders");
                return Enumerable.Empty<PurchaseOrderDto>();
            }
        }

        public async Task<PurchaseOrderDto> GetPurchaseOrderByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/purchaseorders/{id}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<PurchaseOrderDto>(_jsonOptions)
                    ?? new PurchaseOrderDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase order with ID: {Id}", id);
                throw;
            }
        }

        public async Task<PurchaseOrderDto> GetPurchaseOrderByOrderNumberAsync(string orderNumber)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/purchaseorders/by-order-number/{orderNumber}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<PurchaseOrderDto>(_jsonOptions)
                    ?? new PurchaseOrderDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase order with order number: {OrderNumber}", orderNumber);
                throw;
            }
        }

        public async Task<IEnumerable<PurchaseOrderDto>> GetPurchaseOrdersBySupplierAsync(int supplierId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/purchaseorders/by-supplier/{supplierId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<PurchaseOrderDto>>(_jsonOptions)
                    ?? Enumerable.Empty<PurchaseOrderDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase orders for supplier ID: {SupplierId}", supplierId);
                return Enumerable.Empty<PurchaseOrderDto>();
            }
        }

        public async Task<IEnumerable<PurchaseOrderDto>> GetPurchaseOrdersByStatusAsync(PurchaseOrderStatus status)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/purchaseorders/by-status/{status}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<PurchaseOrderDto>>(_jsonOptions)
                    ?? Enumerable.Empty<PurchaseOrderDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase orders with status: {Status}", status);
                return Enumerable.Empty<PurchaseOrderDto>();
            }
        }

        public async Task<IEnumerable<PurchaseOrderDto>> SearchPurchaseOrdersAsync(
            int? supplierId, PurchaseOrderStatus? status,
            DateTime? startDate, DateTime? endDate,
            decimal? minAmount, decimal? maxAmount)
        {
            try
            {
                var queryParams = new List<string>();
                if (supplierId.HasValue) queryParams.Add($"supplierId={supplierId}");
                if (status.HasValue) queryParams.Add($"status={status}");
                if (startDate.HasValue) queryParams.Add($"startDate={startDate:o}");
                if (endDate.HasValue) queryParams.Add($"endDate={endDate:o}");
                if (minAmount.HasValue) queryParams.Add($"minAmount={minAmount}");
                if (maxAmount.HasValue) queryParams.Add($"maxAmount={maxAmount}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

                var response = await _httpClient.GetAsync($"api/v2/purchaseorders/search{queryString}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<PurchaseOrderDto>>(_jsonOptions)
                    ?? Enumerable.Empty<PurchaseOrderDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching purchase orders");
                return Enumerable.Empty<PurchaseOrderDto>();
            }
        }

        public async Task<PurchaseOrderSummaryDto> GetPurchaseOrderSummaryAsync(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var queryParams = new List<string>();
                if (startDate.HasValue) queryParams.Add($"startDate={startDate:o}");
                if (endDate.HasValue) queryParams.Add($"endDate={endDate:o}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

                var response = await _httpClient.GetAsync($"api/v2/purchaseorders/summary{queryString}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<PurchaseOrderSummaryDto>(_jsonOptions)
                    ?? new PurchaseOrderSummaryDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase order summary");
                return new PurchaseOrderSummaryDto();
            }
        }

        public async Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/v1/purchaseorders", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<PurchaseOrderDto>(_jsonOptions)
                    ?? new PurchaseOrderDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase order");
                throw;
            }
        }

        public async Task<PurchaseOrderDto> UpdatePurchaseOrderStatusAsync(int id, UpdatePurchaseOrderStatusDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/v1/purchaseorders/{id}/status", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<PurchaseOrderDto>(_jsonOptions)
                    ?? new PurchaseOrderDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchase order status for ID: {Id}", id);
                throw;
            }
        }

        public async Task<PurchaseOrderDto> ReceivePurchaseOrderItemAsync(int id, int itemId, ReceivePurchaseOrderItemDto dto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(dto, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"api/v1/purchaseorders/{id}/items/{itemId}/receive", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<PurchaseOrderDto>(_jsonOptions)
                    ?? new PurchaseOrderDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving purchase order item for order ID: {Id}, item ID: {ItemId}", id, itemId);
                throw;
            }
        }

        public async Task<PurchaseOrderDto> CancelPurchaseOrderAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/v1/purchaseorders/{id}/cancel", null);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<PurchaseOrderDto>(_jsonOptions)
                    ?? new PurchaseOrderDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling purchase order with ID: {Id}", id);
                throw;
            }
        }
    }
}