/**
 * Cart page functionality
 */
$(document).ready(function () {
    // Initialize item prices display
    initializeItemPrices();
    
    // Initial calculation
    calculateTotals();
    
    // Handle checkbox changes
    $('.item-checkbox').change(function() {
        calculateTotals();
    });
    
    // Select All functionality
    $('#selectAll').change(function() {
        // Check or uncheck all items based on selectAll state
        $('.item-checkbox').prop('checked', $(this).prop('checked'));
        
        // Recalculate totals
        calculateTotals();
    });
    
    // Make sure selectAll stays in sync with individual checkboxes
    $(document).on('change', '.item-checkbox', function() {
        // If any checkbox is unchecked, uncheck the selectAll
        if (!$(this).prop('checked')) {
            $('#selectAll').prop('checked', false);
        }
        // If all checkboxes are checked, check the selectAll
        else if ($('.item-checkbox:checked').length === $('.item-checkbox').length) {
            $('#selectAll').prop('checked', true);
        }
    });
    
    /**
     * Calculate cart totals based on selected items
     */
    function calculateTotals() {
        // Calculate subtotal based on checked items
        var subtotal = 0;
        var selectedItemsCount = 0;
        var selectedItems = [];
        
        $('.item-checkbox:checked').each(function() {
            var row = $(this).closest('tr');
            var price = parseFloat(row.data('price'));
            var quantity = parseInt(row.data('quantity'));
            var itemId = $(this).data('itemid');
            
            subtotal += price * quantity;
            selectedItemsCount += quantity;
            selectedItems.push(itemId);
        });
        
        // Format the subtotal to 2 decimal places
        var formattedSubtotal = subtotal.toFixed(2);
        
        // Update shipping fee if needed (currently fixed at 0)
        var shippingFee = 0;
        
        // Calculate total
        var total = subtotal + shippingFee;
        var formattedTotal = total.toFixed(2);
        
        // Update the DOM
        $('#selected-items-count').text(selectedItemsCount);
        $('#subtotal-price').text(formattedSubtotal);
        $('#shipping-fee').text(shippingFee.toFixed(2));
        $('#total-price').text(formattedTotal);
        $('#checkout-items-count').text(selectedItemsCount);
        
        // Update hidden inputs for form submission
        $('#selectedItemsContainer').empty();
        $.each(selectedItems, function(index, itemId) {
            $('#selectedItemsContainer').append(
                $('<input>').attr({
                    type: 'hidden',
                    name: 'selectedItems',
                    value: itemId
                })
            );
        });
        
        // Enable/disable checkout button based on selection
        if (selectedItemsCount > 0) {
            $('#checkout-btn').removeClass('disabled');
        } else {
            $('#checkout-btn').addClass('disabled');
        }
    }
    
    /**
     * Initialize the display of item prices based on quantity
     */
    function initializeItemPrices() {
        $('.cart-item-row').each(function() {
            var row = $(this);
            var unitPrice = parseFloat(row.data('price'));
            var quantity = parseInt(row.data('quantity'));
            var totalPrice = unitPrice * quantity;
            
            // For regular price items
            if (!row.find('.text-decoration-line-through').length) {
                row.find('td.text-end span').text('NPR ' + totalPrice.toFixed(2));
            }
        });
    }
    
    /**
     * Handle quantity changes
     */
    $('.quantity-btn').click(function() {
        if ($(this).prop('disabled')) {
            return; // Don't proceed if button is disabled
        }
        
        var action = $(this).data('action');
        var itemId = $(this).data('id');
        var change = action === 'increase' ? 1 : -1;
        
        // Get current quantity
        var currentQty = parseInt($('.quantity-display-' + itemId).text());
        
        // Additional check to prevent decreasing below 1
        if (action === 'decrease' && currentQty <= 1) {
            return;
        }
        
        // AJAX call to update quantity
        updateItemQuantity(itemId, change, currentQty);
    });
    
    /**
     * Update quantity via AJAX
     */
    function updateItemQuantity(itemId, change, currentQty) {
        $.ajax({
            url: cartUrls.updateQuantity,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                cartItemId: itemId,
                change: change
            }),
            success: function(response) {
                if (response.success) {
                    // Calculate the new quantity
                    var newQty = currentQty + change;
                    
                    // Update the displayed quantity
                    $('.quantity-display-' + itemId).text(newQty);
                    
                    // Get the cart item row
                    var row = $('tr[data-itemid="' + itemId + '"]');
                    
                    // Update the data-quantity attribute for calculations
                    row.attr('data-quantity', newQty);
                    // FIXED: Also update jQuery's data cache so calculateTotals gets the new value
                    row.data('quantity', newQty);
                    
                    // Get the unit price from data attribute
                    var unitPrice = parseFloat(row.data('price'));
                    
                    // Calculate the new total price for this item
                    var totalPrice = unitPrice * newQty;
                    
                    // Update the price display
                    updateItemPriceDisplay(row, totalPrice);
                    
                    // Update disabled state of buttons based on new quantity and stock
                    if (typeof response.maxStock !== 'undefined') {
                        updateQuantityButtonStates(itemId, newQty, response.maxStock);
                    } else {
                        // Fallback if maxStock not available
                        if (newQty <= 1) {
                            $('button[data-action="decrease"][data-id="' + itemId + '"]').prop('disabled', true);
                        } else {
                            $('button[data-action="decrease"][data-id="' + itemId + '"]').prop('disabled', false);
                        }
                    }
                    
                    // Recalculate cart totals
                    calculateTotals();
                }
            }
        });
    }
    
    /**
     * Update item price display based on quantity
     */
    function updateItemPriceDisplay(row, totalPrice) {
        var priceColumn = row.find('td.text-end');
        
        // Check if there's a discounted price
        if (priceColumn.find('.text-decoration-line-through').length > 0) {
            // Item has a discount
            var originalPrice = parseFloat(row.data('original-price'));
            var quantity = parseInt(row.attr('data-quantity'));
            var originalTotalPrice = originalPrice * quantity;
            
            // Update the displayed prices
            priceColumn.find('.text-decoration-line-through').text('NPR ' + originalTotalPrice.toFixed(2));
            priceColumn.find('.text-danger').text('NPR ' + totalPrice.toFixed(2));
        } else {
            // No discount
            priceColumn.find('span').text('NPR ' + totalPrice.toFixed(2));
        }
    }
    
    /**
     * Update button states based on quantity
     */
    function updateQuantityButtonStates(itemId, newQty, maxStock) {
        // Disable decrease button if quantity is 1
        if (newQty <= 1) {
            $('button[data-action="decrease"][data-id="' + itemId + '"]').prop('disabled', true);
        } else {
            $('button[data-action="decrease"][data-id="' + itemId + '"]').prop('disabled', false);
        }
        
        // Disable increase button if quantity equals or exceeds stock
        if (maxStock !== undefined && newQty >= maxStock) {
            $('button[data-action="increase"][data-id="' + itemId + '"]').prop('disabled', true);
        } else {
            $('button[data-action="increase"][data-id="' + itemId + '"]').prop('disabled', false);
        }
    }

    /**
     * Handle single item delete button clicks
     */
    $('.item-delete-btn').click(function() {
        var itemId = $(this).data('itemid');
        var itemTitle = $(this).data('itemtitle');
        
        // Set modal for single item deletion
        showDeleteModal('single', itemId, itemTitle);
    });

    /**
     * Handle bulk delete button click
     */
    $('#deleteSelected').click(function() {
        var selectedItems = [];
        
        $('.item-checkbox:checked').each(function() {
            selectedItems.push($(this).data('itemid'));
        });
        
        // Show modal with appropriate content based on selection
        showDeleteModal('bulk', selectedItems);
    });
    
    /**
     * Configure and display delete confirmation modal
     */
    function showDeleteModal(mode, itemsData, itemTitle) {
        if (mode === 'single') {
            // Set modal for single item deletion
            $('#deleteConfirmationModal').data('deleteMode', 'single');
            $('#deleteConfirmationModal').data('singleItemId', itemsData);
            
            // Update modal content for single item deletion
            $('#deleteConfirmationModal .modal-header').removeClass('bg-warning').addClass('bg-danger');
            $('#deleteConfirmationModalLabel').text('Confirm Deletion');
            $('#deleteConfirmationModal .modal-body').html(
                '<div class="text-center mb-3">' +
                '<i class="fas fa-trash fa-2x text-danger mb-3"></i>' +
                '<h5>Remove Item from Cart?</h5>' +
                '<p>Are you sure you want to remove <strong>' + itemTitle + '</strong> from your cart?</p>' +
                '<p class="text-muted small">This action cannot be undone.</p>' +
                '</div>'
            );
            $('#confirmDelete').show();
        } else {
            // Set modal for bulk deletion
            $('#deleteConfirmationModal').data('deleteMode', 'bulk');
            $('#deleteConfirmationModal').data('selectedItems', itemsData);
            
            // Change modal content based on whether items are selected
            if (itemsData.length === 0) {
                // No items selected - show warning message with icon
                $('#deleteConfirmationModal .modal-header').removeClass('bg-danger').addClass('bg-warning');
                $('#deleteConfirmationModalLabel').text('No Items Selected');
                $('#deleteConfirmationModal .modal-body').html(
                    '<div class="text-center py-3">' +
                    '<i class="fas fa-exclamation-triangle fa-3x text-warning mb-3"></i>' +
                    '<h5>Please select at least one item to delete.</h5>' +
                    '<p class="text-muted">Use the checkboxes to select items from your cart.</p>' +
                    '</div>'
                );
                $('#confirmDelete').hide(); // Hide the delete button
            } else {
                // Items selected - show normal confirmation message
                $('#deleteConfirmationModal .modal-header').removeClass('bg-warning').addClass('bg-danger');
                $('#deleteConfirmationModalLabel').text('Confirm Deletion');
                $('#deleteConfirmationModal .modal-body').html(
                    '<p>Are you sure you want to remove the selected items from your cart?</p>' +
                    '<p class="text-muted small">This action cannot be undone.</p>'
                );
                $('#confirmDelete').show(); // Show the delete button
            }
        }
        
        // Show the custom modal
        $('#deleteConfirmationModal').modal('show');
    }
    
    /**
     * Handle confirm delete button in modal
     */
    $('#confirmDelete').click(function() {
        var deleteMode = $('#deleteConfirmationModal').data('deleteMode');
        
        if (deleteMode === 'single') {
            // Handle single item deletion
            var itemId = $('#deleteConfirmationModal').data('singleItemId');
            
            // AJAX call to delete single item
            $.ajax({
                url: cartUrls.removeItemAjax,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    cartItemId: itemId
                }),
                headers: {
                    'RequestVerificationToken': $('input:hidden[name="__RequestVerificationToken"]').val()
                },
                success: function(response) {
                    handleDeleteResponse(response, 'Item removed from cart');
                },
                error: handleDeleteError
            });
        } else {
            // Handle bulk deletion (multiple items)
            var selectedItems = $('#deleteConfirmationModal').data('selectedItems');
            
            // AJAX call to delete selected items
            $.ajax({
                url: cartUrls.deleteSelectedItems,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(selectedItems),
                headers: {
                    'RequestVerificationToken': $('input:hidden[name="__RequestVerificationToken"]').val()
                },
                success: function(response) {
                    handleDeleteResponse(response);
                },
                error: handleDeleteError
            });
        }
    });
    
    /**
     * Handle successful deletion response
     */
    function handleDeleteResponse(response, defaultMessage) {
        // Hide the modal
        $('#deleteConfirmationModal').modal('hide');
        
        if (response.success) {
            // Show success message as toast or notification
            $('#toastMessage').text(response.message || defaultMessage || 'Items removed successfully');
            var toastEl = $('#successToast');
            var toast = new bootstrap.Toast(toastEl);
            toast.show();
            
            // Reload the page to update the cart
            setTimeout(function() {
                location.reload();
            }, 500);
        } else {
            alert(response.message || 'Failed to remove items');
        }
    }
    
    /**
     * Handle deletion error
     */
    function handleDeleteError() {
        // Hide the modal
        $('#deleteConfirmationModal').modal('hide');
        alert('An error occurred while trying to remove items');
    }
});