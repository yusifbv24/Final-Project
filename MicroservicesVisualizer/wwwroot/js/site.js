// site.js - Common functionality
document.addEventListener('DOMContentLoaded', function () {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });

    // Initialize toasts
    var toastElList = [].slice.call(document.querySelectorAll('.toast'))
    var toastList = toastElList.map(function (toastEl) {
        return new bootstrap.Toast(toastEl)
    });
});

// Create a toast notification
function showNotification(title, message, type = 'info') {
    // Create container if it doesn't exist
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }

    // Create toast element
    const toastId = 'toast-' + Date.now();
    const toastHtml = `
        <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header bg-${type} text-white">
                <strong class="me-auto">${title}</strong>
                <small>${new Date().toLocaleTimeString()}</small>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;

    // Add toast to container
    toastContainer.innerHTML += toastHtml;

    // Initialize and show toast
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement);
    toast.show();

    // Remove toast after it's hidden
    toastElement.addEventListener('hidden.bs.toast', function () {
        toastElement.remove();
    });

    // Add to activity log if on dashboard
    const activityLog = document.getElementById('activityLog');
    if (activityLog) {
        const noActivities = document.getElementById('noActivities');
        if (noActivities) {
            noActivities.style.display = 'none';
        }

        const activityItem = document.createElement('div');
        activityItem.className = `alert alert-${type} alert-sm mb-2`;
        activityItem.innerHTML = `
            <small class="text-muted">${new Date().toLocaleTimeString()}</small>
            <strong class="ms-2">${title}</strong>: ${message}
        `;
        activityLog.prepend(activityItem);

        // Limit activity log items
        if (activityLog.children.length > 10) {
            activityLog.removeChild(activityLog.lastChild);
        }
    }
}