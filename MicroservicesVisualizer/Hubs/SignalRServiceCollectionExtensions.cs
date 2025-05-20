using Microsoft.AspNetCore.SignalR.Client;
using MicroservicesVisualizer.Models.Inventory;
using MicroservicesVisualizer.Models.Order;
using MicroservicesVisualizer.Models.Supplier;
using Microsoft.AspNetCore.SignalR;

namespace MicroservicesVisualizer.Hubs
{
    public static class SignalRServiceCollectionExtensions
    {
        public static IServiceCollection AddSignalRServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add SignalR core services
            services.AddSignalR();

            // Optionally register background service for connecting to microservice hubs
            services.AddHostedService<MicroserviceHubConnectionService>();

            return services;
        }
    }

    // Background service to maintain connections to microservice SignalR hubs
    public class MicroserviceHubConnectionService : BackgroundService
    {
        private readonly ILogger<MicroserviceHubConnectionService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<NotificationHub> _notificationHub;

        private HubConnection? _inventoryHubConnection;
        private HubConnection? _orderHubConnection;
        private HubConnection? _productHubConnection;
        private HubConnection? _supplierHubConnection;

        public MicroserviceHubConnectionService(
            ILogger<MicroserviceHubConnectionService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IHubContext<NotificationHub> notificationHub)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _notificationHub = notificationHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Initialize connections to all microservice hubs
                await InitializeHubConnections(stoppingToken);

                // Keep the service running
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Check connections periodically and reconnect if needed
                    await CheckAndReconnectHubs(stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            catch (Exception ex) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown, log any issues
                _logger.LogInformation("MicroserviceHubConnectionService is shutting down: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MicroserviceHubConnectionService");
            }
            finally
            {
                // Clean up connections on service shutdown
                await DisconnectHubs();
            }
        }

        private async Task InitializeHubConnections(CancellationToken cancellationToken)
        {
            try
            {
                // Get hub URLs from configuration
                var inventoryHubUrl = _configuration["SignalRUrls:InventoryHub"];
                var orderHubUrl = _configuration["SignalRUrls:OrderHub"];
                var productHubUrl = _configuration["SignalRUrls:ProductHub"];
                var supplierHubUrl = _configuration["SignalRUrls:SupplierHub"];

                // Initialize hub connections
                if (!string.IsNullOrEmpty(inventoryHubUrl))
                {
                    _inventoryHubConnection = new HubConnectionBuilder()
                        .WithUrl(inventoryHubUrl)
                        .WithAutomaticReconnect()
                        .Build();

                    // Set up event handlers to forward events to our local hub
                    SetupInventoryEvents(_inventoryHubConnection);
                    await _inventoryHubConnection.StartAsync(cancellationToken);
                    _logger.LogInformation("Connected to Inventory Hub at {Url}", inventoryHubUrl);
                }

                if (!string.IsNullOrEmpty(orderHubUrl))
                {
                    _orderHubConnection = new HubConnectionBuilder()
                        .WithUrl(orderHubUrl)
                        .WithAutomaticReconnect()
                        .Build();

                    SetupOrderEvents(_orderHubConnection);
                    await _orderHubConnection.StartAsync(cancellationToken);
                    _logger.LogInformation("Connected to Order Hub at {Url}", orderHubUrl);
                }

                if (!string.IsNullOrEmpty(productHubUrl))
                {
                    _productHubConnection = new HubConnectionBuilder()
                        .WithUrl(productHubUrl)
                        .WithAutomaticReconnect()
                        .Build();

                    SetupProductEvents(_productHubConnection);
                    await _productHubConnection.StartAsync(cancellationToken);
                    _logger.LogInformation("Connected to Product Hub at {Url}", productHubUrl);
                }

                if (!string.IsNullOrEmpty(supplierHubUrl))
                {
                    _supplierHubConnection = new HubConnectionBuilder()
                        .WithUrl(supplierHubUrl)
                        .WithAutomaticReconnect()
                        .Build();

                    SetupSupplierEvents(_supplierHubConnection);
                    await _supplierHubConnection.StartAsync(cancellationToken);
                    _logger.LogInformation("Connected to Supplier Hub at {Url}", supplierHubUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing hub connections");
            }
        }

        private void SetupInventoryEvents(HubConnection hubConnection)
        {
            hubConnection.On<int, int, int>("InventoryUpdated", async (inventoryId, productId, quantity) =>
            {
                await _notificationHub.Clients.All.SendAsync("InventoryUpdated", inventoryId, productId, quantity);
            });

            hubConnection.On<int, int, int, string, int>("InventoryTransactionCreated", async (transactionId, inventoryId, productId, type, quantity) =>
            {
                await _notificationHub.Clients.All.SendAsync("InventoryTransactionCreated",
                    transactionId, inventoryId, productId, type, quantity);
            });

            hubConnection.On<int, int, int, int, int>("LowStockAlert", async (inventoryId, productId, locationId, quantity, threshold) =>
            {
                await _notificationHub.Clients.All.SendAsync("LowStockAlert",
                    inventoryId, productId, locationId, quantity, threshold);
            });
        }

        private void SetupOrderEvents(HubConnection hubConnection)
        {
            hubConnection.On<int, int, decimal>("OrderCreated", async (orderId, customerId, totalAmount) =>
            {
                await _notificationHub.Clients.All.SendAsync("OrderCreated", orderId, customerId, totalAmount);
            });

            hubConnection.On<int, string, string>("OrderStatusChanged", async (orderId, oldStatus, newStatus) =>
            {
                await _notificationHub.Clients.All.SendAsync("OrderStatusChanged", orderId, oldStatus, newStatus);
            });

            hubConnection.On<int>("OrderCancelled", async (orderId) =>
            {
                await _notificationHub.Clients.All.SendAsync("OrderCancelled", orderId);
            });
        }

        private void SetupProductEvents(HubConnection hubConnection)
        {
            hubConnection.On<int, string>("ProductCreated", async (productId, name) =>
            {
                await _notificationHub.Clients.All.SendAsync("ProductCreated", productId, name);
            });

            hubConnection.On<int, string>("ProductUpdated", async (productId, name) =>
            {
                await _notificationHub.Clients.All.SendAsync("ProductUpdated", productId, name);
            });

            hubConnection.On<int>("ProductDeleted", async (productId) =>
            {
                await _notificationHub.Clients.All.SendAsync("ProductDeleted", productId);
            });
        }

        private void SetupSupplierEvents(HubConnection hubConnection)
        {
            hubConnection.On<int, int, string>("PurchaseOrderCreated", async (purchaseOrderId, supplierId, orderNumber) =>
            {
                await _notificationHub.Clients.All.SendAsync("PurchaseOrderCreated",
                    purchaseOrderId, supplierId, orderNumber);
            });

            hubConnection.On<int, string, string>("PurchaseOrderStatusChanged", async (purchaseOrderId, oldStatus, newStatus) =>
            {
                await _notificationHub.Clients.All.SendAsync("PurchaseOrderStatusChanged",
                    purchaseOrderId, oldStatus, newStatus);
            });

            hubConnection.On<int, int, int>("PurchaseOrderItemReceived", async (purchaseOrderId, itemId, receivedQuantity) =>
            {
                await _notificationHub.Clients.All.SendAsync("PurchaseOrderItemReceived",
                    purchaseOrderId, itemId, receivedQuantity);
            });
        }

        private async Task CheckAndReconnectHubs(CancellationToken cancellationToken)
        {
            try
            {
                // Check and reconnect inventory hub if needed
                if (_inventoryHubConnection != null &&
                    _inventoryHubConnection.State != HubConnectionState.Connected &&
                    _inventoryHubConnection.State != HubConnectionState.Connecting &&
                    _inventoryHubConnection.State != HubConnectionState.Reconnecting)
                {
                    _logger.LogInformation("Reconnecting to Inventory Hub...");
                    await _inventoryHubConnection.StartAsync(cancellationToken);
                }

                // Check and reconnect order hub if needed
                if (_orderHubConnection != null &&
                    _orderHubConnection.State != HubConnectionState.Connected &&
                    _orderHubConnection.State != HubConnectionState.Connecting &&
                    _orderHubConnection.State != HubConnectionState.Reconnecting)
                {
                    _logger.LogInformation("Reconnecting to Order Hub...");
                    await _orderHubConnection.StartAsync(cancellationToken);
                }

                // Check and reconnect product hub if needed
                if (_productHubConnection != null &&
                    _productHubConnection.State != HubConnectionState.Connected &&
                    _productHubConnection.State != HubConnectionState.Connecting &&
                    _productHubConnection.State != HubConnectionState.Reconnecting)
                {
                    _logger.LogInformation("Reconnecting to Product Hub...");
                    await _productHubConnection.StartAsync(cancellationToken);
                }

                // Check and reconnect supplier hub if needed
                if (_supplierHubConnection != null &&
                    _supplierHubConnection.State != HubConnectionState.Connected &&
                    _supplierHubConnection.State != HubConnectionState.Connecting &&
                    _supplierHubConnection.State != HubConnectionState.Reconnecting)
                {
                    _logger.LogInformation("Reconnecting to Supplier Hub...");
                    await _supplierHubConnection.StartAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and reconnecting hubs");
            }
        }

        private async Task DisconnectHubs()
        {
            try
            {
                if (_inventoryHubConnection != null)
                {
                    await _inventoryHubConnection.DisposeAsync();
                    _logger.LogInformation("Disconnected from Inventory Hub");
                }

                if (_orderHubConnection != null)
                {
                    await _orderHubConnection.DisposeAsync();
                    _logger.LogInformation("Disconnected from Order Hub");
                }

                if (_productHubConnection != null)
                {
                    await _productHubConnection.DisposeAsync();
                    _logger.LogInformation("Disconnected from Product Hub");
                }

                if (_supplierHubConnection != null)
                {
                    await _supplierHubConnection.DisposeAsync();
                    _logger.LogInformation("Disconnected from Supplier Hub");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from hub connections");
            }
        }
    }
}