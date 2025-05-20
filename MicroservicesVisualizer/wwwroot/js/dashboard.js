// dashboard.js - Dashboard specific functionality
document.addEventListener('DOMContentLoaded', function () {
    // Connect to our local NotificationHub
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/notificationHub')
        .withAutomaticReconnect()
        .build();

    // Common connection handling
    connection.onclose(error => {
        console.error('SignalR connection closed', error);
        if (error) {
            showNotification('Connection Lost', 'The real-time connection has been lost. Attempting to reconnect...', 'danger');
        }
    });

    connection.onreconnecting(error => {
        console.warn('SignalR reconnecting', error);
        showNotification('Reconnecting', 'Attempting to restore the real-time connection...', 'warning');
    });

    connection.onreconnected(connectionId => {
        console.log('SignalR reconnected', connectionId);
        showNotification('Reconnected', 'The real-time connection has been restored.', 'success');
    });

    // Start the connection
    connection.start()
        .then(() => {
            console.log('SignalR connected.');
            showNotification('Connected', 'Real-time notifications are enabled.', 'success');
        })
        .catch(err => {
            console.error('Error connecting to SignalR hub:', err);
            showNotification('Connection Error', 'Failed to establish real-time connection.', 'danger');
        });
});