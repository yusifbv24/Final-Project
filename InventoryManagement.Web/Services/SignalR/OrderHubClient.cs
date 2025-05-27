using Microsoft.AspNetCore.SignalR.Client;

namespace InventoryManagement.Web.Services.SignalR
{
    public class OrderHubClient : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly ILogger<OrderHubClient> _logger;
        private bool _isConnected = false;

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

            // Set up event handlers
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

            // Add connection state handlers 
            _hubConnection.Closed += async (error) =>
            {
                _isConnected = false;
                _logger.LogWarning("Connection to Order hub closed. Error: {Error}", error?.Message);
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await StartAsync();
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                _isConnected = true;
                _logger.LogInformation("Reconnected to Order hub. ConnectionId: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

            _hubConnection.Reconnecting += (error) =>
            {
                _isConnected = false;
                _logger.LogWarning("Reconnecting to Order hub. Error: {Error}", error?.Message);
                return Task.CompletedTask;
            };
        }

        public async Task StartAsync()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await _hubConnection.StartAsync();
                    _isConnected = true;
                    _logger.LogInformation("Connected to Order hub");
                }
                catch (Exception ex)
                {
                    _isConnected = false;
                    _logger.LogError(ex, "Error connecting to Order hub");

                    // Retry after 5 seconds
                    await Task.Delay(5000);
                    await StartAsync();
                }
            }
        }

        public bool IsConnected => _isConnected && _hubConnection.State == HubConnectionState.Connected;

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
