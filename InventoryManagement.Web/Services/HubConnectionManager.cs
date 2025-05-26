using InventoryManagement.Web.Services.SignalR;

namespace InventoryManagement.Web.Services
{
    public class HubConnectionManager : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HubConnectionManager> _logger;

        public HubConnectionManager(IServiceProvider serviceProvider, ILogger<HubConnectionManager> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
            // Wait for the application to start
            await Task.Delay(5000, stoppingToken);

            using var scope = _serviceProvider.CreateScope();

            try
            {
                // Start hub connections
                var productHubClient = _serviceProvider.GetRequiredService<ProductHubClient>();
                var inventoryHubClient = _serviceProvider.GetRequiredService<InventoryHubClient>();
                var orderHubClient = _serviceProvider.GetRequiredService<OrderHubClient>();

                await StartHubConnectionWithRetry(productHubClient.StartAsync, "Product", stoppingToken);
                await StartHubConnectionWithRetry(inventoryHubClient.StartAsync, "Inventory", stoppingToken);
                await StartHubConnectionWithRetry(orderHubClient.StartAsync, "Order", stoppingToken);

                _logger.LogInformation("All SignalR hub clients started successfully");

                // Keep the service running
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HubConnectionManager");
            }
        }

        private async Task StartHubConnectionWithRetry(Func<Task> startFunc, string hubName, CancellationToken cancellationToken)
        {
            int retryCount = 0;
            const int maxRetries = 5;

            while (retryCount < maxRetries && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await startFunc();
                    _logger.LogInformation("{HubName} hub client started successfully", hubName);
                    return;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogWarning(ex, "{HubName} hub client failed to start, retry {RetryCount}/{MaxRetries}",
                        hubName, retryCount, maxRetries);

                    if (retryCount < maxRetries)
                    {
                        await Task.Delay(5000 * retryCount, cancellationToken); // Exponential backoff
                    }
                }
            }

            _logger.LogError("Failed to start {HubName} hub client after {MaxRetries} retries", hubName, maxRetries);
        }
    }
}
