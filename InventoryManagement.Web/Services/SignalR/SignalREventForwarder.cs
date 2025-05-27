using Microsoft.AspNetCore.SignalR;
using ProductService.Application.Hubs;
using InventoryService.Application.Hubs;
using OrderService.Application.Hubs;

namespace InventoryManagement.Web.Services.SignalR
{
    public class SignalREventForwarder : BackgroundService
    {
        private readonly IHubContext<ProductHub> _productHubContext;
        private readonly IHubContext<InventoryHub> _inventoryHubContext;
        private readonly IHubContext<OrderHub> _orderHubContext;
        private readonly ProductHubClient _productHubClient;
        private readonly InventoryHubClient _inventoryHubClient;
        private readonly OrderHubClient _orderHubClient;
        private readonly ILogger<SignalREventForwarder> _logger;

        public SignalREventForwarder(
            IHubContext<ProductHub> productHubContext,
            IHubContext<InventoryHub> inventoryHubContext,
            IHubContext<OrderHub> orderHubContext,
            ProductHubClient productHubClient,
            InventoryHubClient inventoryHubClient,
            OrderHubClient orderHubClient,
            ILogger<SignalREventForwarder> logger)
        {
            _productHubContext = productHubContext;
            _inventoryHubContext = inventoryHubContext;
            _orderHubContext = orderHubContext;
            _productHubClient = productHubClient;
            _inventoryHubClient = inventoryHubClient;
            _orderHubClient = orderHubClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait for services to be ready
            await Task.Delay(2000, stoppingToken);

            // Set up event handlers
            _productHubClient.ProductCreated += async (productId, name) =>
            {
                try
                {
                    await _productHubContext.Clients.All.SendAsync("ProductCreated", productId, name, stoppingToken);
                    _logger.LogInformation("Forwarded ProductCreated: {ProductId} - {Name}", productId, name);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error forwarding ProductCreated");
                }
            };

            _productHubClient.ProductUpdated += async (productId, name) =>
            {
                try
                {
                    await _productHubContext.Clients.All.SendAsync("ProductUpdated", productId, name, stoppingToken);
                    _logger.LogInformation("Forwarded ProductUpdated: {ProductId} - {Name}", productId, name);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error forwarding ProductUpdated");
                }
            };

            _productHubClient.ProductDeleted += async (productId) =>
            {
                try
                {
                    await _productHubContext.Clients.All.SendAsync("ProductDeleted", productId, stoppingToken);
                    _logger.LogInformation("Forwarded ProductDeleted: {ProductId}", productId);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error forwarding ProductDeleted");
                }
            };

            _inventoryHubClient.InventoryUpdated += async (inventoryId, productId, quantity) =>
            {
                try
                {
                    await _inventoryHubContext.Clients.All.SendAsync("InventoryUpdated", inventoryId, productId, quantity, stoppingToken);
                    _logger.LogInformation("Forwarded InventoryUpdated: {InventoryId} - Product {ProductId} - Quantity {Quantity}", inventoryId, productId, quantity);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error forwarding InventoryUpdated");
                }
            };

            _inventoryHubClient.InventoryTransactionCreated += async (transactionId, inventoryId, productId, type, quantity) =>
            {
                try
                {
                    await _inventoryHubContext.Clients.All.SendAsync("InventoryTransactionCreated", transactionId, inventoryId, productId, type, quantity, stoppingToken);
                    _logger.LogInformation("Forwarded InventoryTransactionCreated");
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error forwarding InventoryTransactionCreated");
                }
            };

            _orderHubClient.OrderCreated += async (orderId, customerName) =>
            {
                try
                {
                    await _orderHubContext.Clients.All.SendAsync("OrderCreated", orderId, customerName, stoppingToken);
                    _logger.LogInformation("Forwarded OrderCreated: {OrderId} - {CustomerName}", orderId, customerName);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error forwarding OrderCreated");
                }
            };

            _orderHubClient.OrderStatusChanged += async (orderId, status) =>
            {
                try
                {
                    await _orderHubContext.Clients.All.SendAsync("OrderStatusChanged", orderId, status, stoppingToken);
                    _logger.LogInformation("Forwarded OrderStatusChanged: {OrderId} - {Status}", orderId, status);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error forwarding OrderStatusChanged");
                }
            };

            _logger.LogInformation("SignalR Event Forwarder started and event handlers registered");

            // Keep the service running
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("SignalR Event Forwarder stopping...");
            }
        }
    }
}
