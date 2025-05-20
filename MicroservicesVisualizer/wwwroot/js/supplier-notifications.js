// supplier-notifications.js - Supplier SignalR notifications
document.addEventListener('DOMContentLoaded', function () {
    // Connect to SupplierHub on the Supplier Service
    const supplierServiceUrl = document.querySelector('meta[name="supplier-service-url"]')?.content || 'http://localhost:5281/hubs/suppliers';

    const supplierConnection = new signalR.HubConnectionBuilder()
        .withUrl(supplierServiceUrl)
        .withAutomaticReconnect()
        .build();

    // Set up supplier event handlers
    supplierConnection.on('PurchaseOrderCreated', (purchaseOrderId, supplierId, orderNumber) => {
        console.log('Purchase order created:', { purchaseOrderId, supplierId, orderNumber });
        showNotification('New Purchase Order', `PO #${orderNumber} (ID: ${purchaseOrderId}) created`, 'success');
    });

    supplierConnection.on('PurchaseOrderStatusChanged', (purchaseOrderId, oldStatus, newStatus) => {
        console.log('Purchase order status changed:', { purchaseOrderId, oldStatus, newStatus });
        showNotification('PO Status Changed', `PO #${purchaseOrderId}: ${oldStatus} → ${newStatus}`, 'primary');
    });

    supplierConnection.on('PurchaseOrderItemReceived', (purchaseOrderId, itemId, receivedQuantity) => {
        console.log('Purchase order item received:', { purchaseOrderId, itemId, receivedQuantity });
        showNotification('PO Item Received', `PO #${purchaseOrderId}: Received ${receivedQuantity} units for item #${itemId}`, 'info');
    });

    // Start the connection
    supplierConnection.start()
        .then(() => {
            console.log('Connected to Supplier Service SignalR hub.');
        })
        .catch(err => {
            console.error('Error connecting to Supplier Service SignalR hub:', err);
        });

    // Connect to local NotificationHub (which forwards events from our backend)
    const localConnection = new signalR.HubConnectionBuilder()
        .withUrl('/notificationHub')
        .withAutomaticReconnect()
        .build();

    // Set up the same handlers for local connection (these events may be forwarded from our backend)
    localConnection.on('PurchaseOrderCreated', (purchaseOrderId, supplierId, orderNumber) => {
        console.log('Purchase order created (local):', { purchaseOrderId, supplierId, orderNumber });
        showNotification('New Purchase Order', `PO #${orderNumber} (ID: ${purchaseOrderId}) created`, 'success');
    });

    localConnection.on('PurchaseOrderStatusChanged', (purchaseOrderId, oldStatus, newStatus) => {
        console.log('Purchase order status changed (local):', { purchaseOrderId, oldStatus, newStatus });
        showNotification('PO Status Changed', `PO #${purchaseOrderId}: ${oldStatus} → ${newStatus}`, 'primary');
    });

    localConnection.on('PurchaseOrderItemReceived', (purchaseOrderId, itemId, receivedQuantity) => {
        console.log('Purchase order item received (local):', { purchaseOrderId, itemId, receivedQuantity });
        showNotification('PO Item Received', `PO #${purchaseOrderId}: Received ${receivedQuantity} units for item #${itemId}`, 'info');
    });

    // Start the local connection
    localConnection.start()
        .then(() => {
            console.log('Connected to local NotificationHub for supplier events.');
        })
        .catch(err => {
            console.error('Error connecting to local NotificationHub for supplier events:', err);
        });
});