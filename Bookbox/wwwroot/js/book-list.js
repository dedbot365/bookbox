// JavaScript for the book index/listing page

// Update sort function
function updateSort(sortValue) {
    // Get current URL
    var url = new URL(window.location.href);
    
    // Update or add sort parameter
    url.searchParams.set('sortBy', sortValue);
    
    // Make sure we preserve category
    var category = document.getElementById('categoryParam').value;
    if (category) {
        url.searchParams.set('category', category);
    }
    
    // Redirect to new URL
    window.location.href = url.toString();
}

// Document ready function
document.addEventListener('DOMContentLoaded', function() {
    // Set up delete modal with event delegation for dynamic content
    $(document).on('click', '.delete-book', function(e) {
        e.preventDefault();
        e.stopPropagation();
        
        var id = $(this).data('id');
        var title = $(this).data('title');
        var deleteUrl = $(this).data('delete-url');
        
        // Set the form action and book info in the modal
        $('#deleteBookForm').attr('action', deleteUrl + '/' + id);
        $('#delete-book-title').text(title);
        
        // Show the modal
        $('#deleteBookModal').modal('show');
    });
    
    // Check for success message and show modal for admins
    const isAdmin = document.getElementById('isAdminFlag') ? 
        document.getElementById('isAdminFlag').value === 'true' : false;
    const successMessage = document.getElementById('successMessage') ? 
        document.getElementById('successMessage').value : '';
        
    if (isAdmin && successMessage && successMessage.length > 0) {
        // Set the message in the modal
        $('.modal-message').text(successMessage);
        
        // Create and show modal with a slight delay for better UX
        setTimeout(() => {
            const successModal = new bootstrap.Modal(document.getElementById('successModal'));
            successModal.show();
        }, 500);
    }
});