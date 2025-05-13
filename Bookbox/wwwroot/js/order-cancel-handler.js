/**
 * Order Cancellation Handler
 * Handles order cancellation modals and interactions
 */

// Function to show the order cancellation success modal
function showOrderCanceledModal(orderNumber) {
    // Set the order number in the modal
    document.getElementById('orderCanceledNumber').textContent = orderNumber;
    
    // Create and show modal
    const modal = new bootstrap.Modal(document.getElementById('orderCanceledModal'));
    modal.show();
    
    // Setup event listener for when modal is hidden
    document.getElementById('orderCanceledModal').addEventListener('hidden.bs.modal', function() {
        // Optional: If you want to perform any action after modal is closed
        // For example, refresh the page or redirect
        // window.location.reload();
    });
}

// Document ready function
document.addEventListener('DOMContentLoaded', function() {
    // Handle cancel order buttons with AJAX
    document.querySelectorAll('.cancel-order-btn').forEach(button => {
        button.addEventListener('click', function() {
            const orderId = this.getAttribute('data-order-id');
            const orderNumber = this.getAttribute('data-order-number');
            const cancelModalId = `cancelModal${orderId}`;
            const cancelModal = bootstrap.Modal.getInstance(document.getElementById(cancelModalId));
            
            // Create form data
            const formData = new FormData();
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            formData.append('__RequestVerificationToken', token);
            
            // Send cancel request
            fetch(`/Order/Cancel/${orderId}`, {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': token
                }
            })
            .then(response => {
                if (response.ok) {
                    // Hide the cancel confirmation modal
                    cancelModal.hide();
                    
                    // Show the success modal
                    setTimeout(() => {
                        showOrderCanceledModal(orderNumber);
                    }, 300);
                } else {
                    throw new Error('Failed to cancel order');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('An error occurred while trying to cancel the order. Please try again later.');
            });
        });
    });
    
    // Check for TempData message to show modal on page load
    const canceledOrderNumber = document.getElementById('canceledOrderNumber');
    if (canceledOrderNumber && canceledOrderNumber.value) {
        showOrderCanceledModal(canceledOrderNumber.value);
    }
});