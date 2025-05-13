// Additional JavaScript for book details page
// Note: This extends the existing bookDetails.js file with more functionality

$(document).ready(function() {
    // Handle delete button click
    $('.delete-book').on('click', function(e) {
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
});