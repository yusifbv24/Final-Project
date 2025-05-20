// product-notifications.js - Product SignalR notifications
document.addEventListener('DOMContentLoaded', function () {
    // Connect to ProductHub on the Product Service
    const productServiceUrl = document.querySelector('meta[name="product-service-url"]')?.content || 'http://localhost:5104/hubs/products';

    const productConnection = new signalR.HubConnectionBuilder()
        .withUrl(productServiceUrl)
        .withAutomaticReconnect()
        .build();

    // Set up product event handlers
    productConnection.on('ProductCreated', (productId, name) => {
        console.log('Product created:', { productId, name });
        showNotification('New Product', `Product "${name}" (ID: ${productId}) created`, 'success');
    });

    productConnection.on('ProductUpdated', (productId, name) => {
        console.log('Product updated:', { productId, name });
        showNotification('Product Updated', `Product "${name}" (ID: ${productId}) updated`, 'info');
    });

    productConnection.on('ProductDeleted', (productId) => {
        console.log('Product deleted:', { productId });
        showNotification('Product Deleted', `Product #${productId} has been deleted`, 'warning');
    });

    // Start the connection
    productConnection.start()
        .then(() => {
            console.log('Connected to Product Service SignalR hub.');
        })
        .catch(err => {
            console.error('Error connecting to Product Service SignalR hub:', err);
        });

    // Connect to local NotificationHub (which forwards events from our backend)
    const localConnection = new signalR.HubConnectionBuilder()
        .withUrl('/notificationHub')
        .withAutomaticReconnect()
        .build();

    // Set up the same handlers for local connection (these events may be forwarded from our backend)
    localConnection.on('ProductCreated', (productId, name) => {
        console.log('Product created (local):', { productId, name });
        showNotification('New Product', `Product "${name}" (ID: ${productId}) created`, 'success');
    });

    localConnection.on('ProductUpdated', (productId, name) => {
        console.log('Product updated (local):', { productId, name });
        showNotification('Product Updated', `Product "${name}" (ID: ${productId}) updated`, 'info');
    });

    localConnection.on('ProductDeleted', (productId) => {
        console.log('Product deleted (local):', { productId });
        showNotification('Product Deleted', `Product #${productId} has been deleted`, 'warning');
    });

    // Start the local connection
    localConnection.start()
        .then(() => {
            console.log('Connected to local NotificationHub for product events.');
        })
        .catch(err => {
            console.error('Error connecting to local NotificationHub for product events:', err);
        });
});