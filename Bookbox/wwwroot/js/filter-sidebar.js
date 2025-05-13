// Filter Sidebar JS functionality
$(document).ready(function() {
    // Initialize any date pickers if needed
    if($.fn.datepicker) {
        $('.date-picker').datepicker({
            format: 'yyyy-mm-dd',
            autoclose: true,
            todayHighlight: true
        });
    }
    
    // Clear individual filter fields
    $('.clear-filter').on('click', function(e) {
        e.preventDefault();
        const targetId = $(this).data('target');
        $(targetId).val('');
    });
    
    // Price range validator
    $('#filterForm').on('submit', function(e) {
        const minPrice = parseFloat($('#minPrice').val()) || 0;
        const maxPrice = parseFloat($('#maxPrice').val()) || 0;
        
        if (maxPrice > 0 && minPrice > maxPrice) {
            e.preventDefault();
            alert('Minimum price cannot be greater than maximum price');
            return false;
        }
        
        return true;
    });
    
    // Animated collapse for mobile view
    const filterToggle = $('#filterToggle');
    const filterContent = $('#filterContent');
    
    if (filterToggle.length && filterContent.length) {
        filterToggle.on('click', function() {
            filterContent.slideToggle(300);
            const icon = $(this).find('i');
            if (icon.hasClass('fa-chevron-down')) {
                icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
            } else {
                icon.removeClass('fa-chevron-up').addClass('fa-chevron-down');
            }
        });
        
        // Reset toggle state on window resize
        $(window).on('resize', function() {
            if ($(window).width() >= 992) {
                filterContent.show();
            }
        });
    }
});
