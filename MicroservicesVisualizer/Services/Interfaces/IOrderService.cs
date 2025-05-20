using MicroservicesVisualizer.Models.Order;

namespace MicroservicesVisualizer.Services.Interfaces
{
    public interface IOrderService
    {
        // Order methods
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId);
        Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status);
        Task<OrderStatisticsDto> GetOrderStatisticsAsync();

        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task<OrderDto> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto dto);
        Task<OrderDto> UpdateOrderAddressAsync(int id, UpdateOrderAddressDto dto);
        Task<OrderDto> CancelOrderAsync(int id);

        // Customer methods
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto> GetCustomerByIdAsync(int id);
        Task<CustomerDto> GetCustomerByEmailAsync(string email);

        Task<CustomerDto> CreateCustomerAsync(CustomerDto dto);
        Task<CustomerDto> UpdateCustomerAsync(int id, CustomerDto dto);
        Task<CustomerDto> ActivateCustomerAsync(int id);
        Task<CustomerDto> DeactivateCustomerAsync(int id);
    }
}
