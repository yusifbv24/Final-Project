// inventory-notifications.js - Inventory SignalR notifications
document.addEventListener('DOMContentLoaded', function () {
    // Connect to InventoryHub on the Inventory Service
    const inventoryServiceUrl = document.querySelector('meta[name="inventory-service-url"]')?.content || 'http://localhost:5105/hubs/inventory';

    const inventoryConnection = new signalR.HubConnectionBuilder()
        .withUrl(inventoryServiceUrl)
        .withAutomaticReconnect()
        .build();

    // Set up inventory event handlers
    inventoryConnection.on('InventoryUpdated', (inventoryId, productId, quantity) => {
        console.log('Inventory updated:', { inventoryId, productId, quantity });
        showNotification('Inventory Updated', `Product #${productId}: Quantity updated to ${quantity}`, 'primary');
    });

    inventoryConnection.on('InventoryTransactionCreated', (transactionId, inventoryId, productId, type, quantity) => {
        console.log('Inventory transaction created:', { transactionId, inventoryId, productId, type, quantity });
        showNotification('New Transaction', `${type}: ${quantity} units of Product #${productId}`, 'info');
    });

    inventoryConnection.on('LowStockAlert', (inventoryId, productId, locationId, quantity, threshold) => {
        console.log('Low stock alert:', { inventoryId, productId, locationId, quantity, threshold });
        showNotification('Low Stock Alert', `Product #${productId}: Only ${quantity} units left (threshold: ${threshold})`, 'danger');
    });

    // Start the connection
    inventoryConnection.start()
        .then(() => {
            console.log('Connected to Inventory Service SignalR hub.');
        })
        .catch(err => {
            console.error('Error connecting to Inventory Service SignalR hub:', err);
        });

    // Connect to local NotificationHub (which forwards events from our backend)
    const localConnection = new signalR.HubConnectionBuilder()
        .withUrl('/notificationHub')
        .withAutomaticReconnect()
        .build();

    // Set up the same handlers for local connection (these events may be forwarded from our backend)
    localConnection.on('InventoryUpdated', (inventoryId, productId, quantity) => {
        console.log('Inventory updated (local):', { inventoryId, productId, quantity });
        showNotification('Inventory Updated', `Product #${productId}: Quantity updated to ${quantity}`, 'primary');
    });

    localConnection.on('InventoryTransactionCreated', (transactionId, inventoryId, productId, type, quantity) => {
        console.log('Inventory transaction created (local):', { transactionId, inventoryId, productId, type, quantity });
        showNotification('New Transaction', `${type}: ${quantity} units of Product #${productId}`, 'info');
    });

    localConnection.on('LowStockAlert', (inventoryId, productId, locationId, quantity, threshold) => {
        console.log('Low stock alert (local):', { inventoryId, productId, locationId, quantity, threshold });
        showNotification('Low Stock Alert', `Product #${productId}: Only ${quantity} units left (threshold: ${threshold})`, 'danger');
    });

    // Start the local connection
    localConnection.start()
        .then(() => {
            console.log('Connected to local NotificationHub for inventory events.');
        })
        .catch(err => {
            console.error('Error connecting to local NotificationHub for inventory events:', err);
        });
});