using Microsoft.AspNetCore.SignalR.Client;

namespace InventoryManagement.Web.Services.SignalR
{
    public class OrderHubClient : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly ILogger<OrderHubClient> _logger;

        public event Action<int, string>? OrderCreated;
        public event Action<int, string>? OrderStatusChanged;

        public OrderHubClient(IConfiguration configuration, ILogger<OrderHubClient> logger)
        {
            _logger = logger;

            var orderServiceUrl = configuration["Services:OrderService"] ?? "http://localhost:5106";
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{orderServiceUrl}/hubs/orders")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<int, string>("OrderCreated", (orderId, customerName) =>
            {
                _logger.LogInformation("Order created: {OrderId} - Customer {CustomerName}", orderId, customerName);
                OrderCreated?.Invoke(orderId, customerName);
            });

            _hubConnection.On<int, string>("OrderStatusChanged", (orderId, status) =>
            {
                _logger.LogInformation("Order status changed: {OrderId} - Status {Status}", orderId, status);
                OrderStatusChanged?.Invoke(orderId, status);
            });
        }

        public async Task StartAsync()
        {
            try
            {
                await _hubConnection.StartAsync();
                _logger.LogInformation("Connected to Order hub");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to Order hub");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
