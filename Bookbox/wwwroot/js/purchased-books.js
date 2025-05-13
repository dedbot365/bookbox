/**
 * Purchased Books Page JavaScript
 * Handles rating functionality and other interactions
 */

/**
 * Modal management for purchased books 
 * Fixes glitching issues by ensuring proper modal rendering
 */

// Wait for document to be fully loaded
document.addEventListener('DOMContentLoaded', function() {
    // Ensure modals are appended to body
    document.querySelectorAll('.review-modal').forEach(modal => {
        if (modal.parentElement !== document.body) {
            document.body.appendChild(modal);
        }
    });
    
    // Handle modal triggers
    document.querySelectorAll('[data-bs-toggle="modal"]').forEach(trigger => {
        trigger.addEventListener('click', function(e) {
            e.preventDefault();
            
            const targetSelector = this.getAttribute('data-bs-target');
            const targetModal = document.querySelector(targetSelector);
            
            // Close any existing modals first
            const openModals = document.querySelectorAll('.modal.show');
            openModals.forEach(openModal => {
                const instance = bootstrap.Modal.getInstance(openModal);
                if (instance) instance.hide();
            });
            
            // Create fresh modal instance
            const modalInstance = new bootstrap.Modal(targetModal, {
                backdrop: 'static',
                keyboard: false,
                focus: true
            });
            
            // Show with slight delay to prevent rendering conflicts
            setTimeout(() => {
                modalInstance.show();
            }, 50);
        });
    });
    
    // Cleanup when modal is hidden
    document.querySelectorAll('.modal').forEach(modal => {
        modal.addEventListener('hidden.bs.modal', function() {
            // Remove inline styles added by Bootstrap
            document.body.style.paddingRight = '';
            document.body.style.overflow = '';
            
            // Remove modal-specific classes
            document.body.classList.remove('modal-open');
            
            // Remove backdrop if it exists
            const backdrops = document.querySelectorAll('.modal-backdrop');
            backdrops.forEach(backdrop => {
                backdrop.remove();
            });
        });
    });
    
    // Star rating functionality
    $('.rating-stars input').change(function() {
        var $this = $(this);
        var $stars = $this.closest('.rating-stars').find('label i');
        var selectedRating = $this.val();
        
        $stars.removeClass('fas').addClass('far');
        
        $this.closest('.rating-stars').find('input').each(function() {
            if ($(this).val() <= selectedRating) {
                $(this).next('label').find('i').removeClass('far').addClass('fas');
            }
        });
    });
    
    // Highlight stars on hover
    $('.rating-stars label').hover(
        function() {
            var $this = $(this);
            var $stars = $this.closest('.rating-stars').find('label i');
            var hoverRating = $this.prev('input').val();
            
            $stars.removeClass('fas hover-star').addClass('far');
            
            $this.closest('.rating-stars').find('input').each(function() {
                if ($(this).val() <= hoverRating) {
                    $(this).next('label').find('i').removeClass('far').addClass('fas hover-star');
                }
            });
        },
        function() {
            var $this = $(this);
            var $container = $this.closest('.rating-stars');
            var $stars = $container.find('label i');
            var selectedRating = $container.find('input:checked').val();
            
            $stars.removeClass('fas hover-star').addClass('far');
            
            if (selectedRating) {
                $container.find('input').each(function() {
                    if ($(this).val() <= selectedRating) {
                        $(this).next('label').find('i').removeClass('far').addClass('fas');
                    }
                });
            }
        }
    );
});