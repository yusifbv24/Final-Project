// order-notifications.js - Order SignalR notifications
document.addEventListener('DOMContentLoaded', function () {
    // Connect to OrderHub on the Order Service
    const orderServiceUrl = document.querySelector('meta[name="order-service-url"]')?.content || 'http://localhost:5155/hubs/orders';

    const orderConnection = new signalR.HubConnectionBuilder()
        .withUrl(orderServiceUrl)
        .withAutomaticReconnect()
        .build();

    // Set up order event handlers
    orderConnection.on('OrderCreated', (orderId, customerId, totalAmount) => {
        console.log('Order created:', { orderId, customerId, totalAmount });
        showNotification('New Order', `Order #${orderId} created: $${totalAmount.toFixed(2)}`, 'success');
    });

    orderConnection.on('OrderStatusChanged', (orderId, oldStatus, newStatus) => {
        console.log('Order status changed:', { orderId, oldStatus, newStatus });
        showNotification('Order Status Changed', `Order #${orderId}: ${oldStatus} → ${newStatus}`, 'primary');
    });

    orderConnection.on('OrderCancelled', (orderId) => {
        console.log('Order cancelled:', { orderId });
        showNotification('Order Cancelled', `Order #${orderId} has been cancelled`, 'warning');
    });

    // Start the connection
    orderConnection.start()
        .then(() => {
            console.log('Connected to Order Service SignalR hub.');
        })
        .catch(err => {
            console.error('Error connecting to Order Service SignalR hub:', err);
        });

    // Connect to local NotificationHub (which forwards events from our backend)
    const localConnection = new signalR.HubConnectionBuilder()
        .withUrl('/notificationHub')
        .withAutomaticReconnect()
        .build();

    // Set up the same handlers for local connection (these events may be forwarded from our backend)
    localConnection.on('OrderCreated', (orderId, customerId, totalAmount) => {
        console.log('Order created (local):', { orderId, customerId, totalAmount });
        showNotification('New Order', `Order #${orderId} created: $${totalAmount.toFixed(2)}`, 'success');
    });

    localConnection.on('OrderStatusChanged', (orderId, oldStatus, newStatus) => {
        console.log('Order status changed (local):', { orderId, oldStatus, newStatus });
        showNotification('Order Status Changed', `Order #${orderId}: ${oldStatus} → ${newStatus}`, 'primary');
    });

    localConnection.on('OrderCancelled', (orderId) => {
        console.log('Order cancelled (local):', { orderId });
        showNotification('Order Cancelled', `Order #${orderId} has been cancelled`, 'warning');
    });

    // Start the local connection
    localConnection.start()
        .then(() => {
            console.log('Connected to local NotificationHub for order events.');
        })
        .catch(err => {
            console.error('Error connecting to local NotificationHub for order events:', err);
        });
});