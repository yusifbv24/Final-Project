using Microsoft.AspNetCore.SignalR.Client;

namespace InventoryManagement.Web.Services.SignalR
{
    public class ProductHubClient : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly ILogger<ProductHubClient> _logger;

        public event Action<int, string>? ProductCreated;
        public event Action<int, string>? ProductUpdated;
        public event Action<int>? ProductDeleted;

        private bool _isConnected = false;
        public bool IsConnected => _isConnected && _hubConnection.State == HubConnectionState.Connected;

        public ProductHubClient(IConfiguration configuration, ILogger<ProductHubClient> logger)
        {
            _logger = logger;

            var productServiceUrl = configuration["Services:ProductService"] ?? "http://localhost:5104";
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{productServiceUrl}/hubs/products")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<int, string>("ProductCreated", (productId, name) =>
            {
                _logger.LogInformation("Product created: {ProductId} - {Name}", productId, name);
                ProductCreated?.Invoke(productId, name);
            });

            _hubConnection.On<int, string>("ProductUpdated", (productId, name) =>
            {
                _logger.LogInformation("Product updated: {ProductId} - {Name}", productId, name);
                ProductUpdated?.Invoke(productId, name);
            });

            _hubConnection.On<int>("ProductDeleted", (productId) =>
            {
                _logger.LogInformation("Product deleted: {ProductId}", productId);
                ProductDeleted?.Invoke(productId);
            });

            _hubConnection.Closed += async (error) => {
                _isConnected = false;
                _logger.LogWarning("Connection to Product hub closed. Error: {Error}", error?.Message);
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await StartAsync();
            };

            _hubConnection.Reconnected += (connectionId) => {
                _isConnected = true;
                _logger.LogInformation("Reconnected to Product hub. ConnectionId: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

            _hubConnection.Reconnecting += (error) => {
                _isConnected = false;
                _logger.LogWarning("Reconnecting to Product hub. Error: {Error}", error?.Message);
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
                    _logger.LogInformation("Connected to Product hub");
                }
                catch (Exception ex)
                {
                    _isConnected = false;
                    _logger.LogError(ex, "Error connecting to Product hub");
                    // Retry after 5 seconds
                    await Task.Delay(5000);
                    await StartAsync();
                }
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
