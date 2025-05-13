/**
 * Purchased Books Page JavaScript
 * Handles rating functionality and other interactions
 */

$(document).ready(function() {
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