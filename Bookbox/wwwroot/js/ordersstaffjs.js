/**
 * Orders Staff JavaScript
 * Handles interactions for the staff orders management page
 */

$(document).ready(function() {
    // Initialize clipboard for copy functionality
    if(typeof ClipboardJS !== 'undefined') {
        new ClipboardJS('.copy-btn').on('success', function(e) {
            // Show tooltip or notification
            $(e.trigger).tooltip({
                title: 'Copied!',
                placement: 'top',
                trigger: 'manual'
            }).tooltip('show');
            
            setTimeout(function() {
                $(e.trigger).tooltip('hide');
            }, 1000);
            
            e.clearSelection();
        });
    }
    
    // Add tooltips to action buttons
    $('[title]').tooltip();
    
    // Handle row highlighting on hover
    $('.order-row').hover(
        function() {
            $(this).addClass('shadow-sm');
        },
        function() {
            $(this).removeClass('shadow-sm');
        }
    );
    
    // Handle filter button group
    $('.btn-group .btn').click(function() {
        $('.btn-group .btn').removeClass('active');
        $(this).addClass('active');
    });
});
