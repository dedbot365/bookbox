/**
 * Order Details Staff JavaScript
 * Handles order details interactions and redemption process
 */

$(document).ready(function() {
    // Handle the redeem order form submission
    $('#redeemOrderForm').on('submit', function(e) {
        e.preventDefault();
        
        // Get form data
        const form = $(this);
        const url = form.attr('action');
        const data = form.serialize();
        const submitButton = form.find('button[type="submit"]');
        
        // Disable button and show loading state
        submitButton.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Processing...');
        
        // Submit the form via AJAX
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function(response) {
                if (response.success) {
                    // Show success modal
                    const successModal = new bootstrap.Modal(document.getElementById('orderSuccessModal'));
                    successModal.show();
                    
                    // Set up continue button to redirect
                    $('#continueButton').on('click', function() {
                        window.location.href = redirectUrl;
                    });
                } else {
                    // Show error alert
                    $('<div class="alert alert-danger alert-dismissible fade show mt-3" role="alert">' +
                        '<i class="fas fa-exclamation-circle me-2"></i>' +
                        response.message +
                        '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
                        '</div>').insertBefore(form);
                    
                    // Reset button
                    submitButton.prop('disabled', false).html('<i class="fas fa-check me-1"></i> Verify & Complete Order');
                }
            },
            error: function() {
                // Show error alert
                $('<div class="alert alert-danger alert-dismissible fade show mt-3" role="alert">' +
                    '<i class="fas fa-exclamation-circle me-2"></i>' +
                    'An error occurred while processing your request. Please try again.' +
                    '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
                    '</div>').insertBefore(form);
                
                // Reset button
                submitButton.prop('disabled', false).html('<i class="fas fa-check me-1"></i> Verify & Complete Order');
            }
        });
    });
    
    // Handle any alert dismissals
    $('.alert .btn-close').on('click', function() {
        $(this).closest('.alert').alert('close');
    });
    
    // Initialize any tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();
});
