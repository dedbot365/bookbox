// Book Details page functionality
$(document).ready(function() {
    // Delete book functionality
    $('.delete-book').on('click', function(e) {
        e.preventDefault();
        e.stopPropagation();
        
        var id = $(this).data('id');
        var title = $(this).data('title');
        
        // Set the form action and book info in the modal
        $('#deleteBookForm').attr('action', '/Book/Delete/' + id);
        $('#delete-book-title').text(title);
        
        // Show the modal
        $('#deleteBookModal').modal('show');
    });
    
    // Handle "View all reviews" button click
    $('#showAllReviews').on('click', function() {
        // Show the modal
        $('#allReviewsModal').modal('show');
        
        // Get book ID from data attribute
        var bookId = $(this).data('book-id');
        
        // Fetch all reviews via AJAX
        $.ajax({
            url: '/Book/GetAllReviews?id=' + bookId + '&count=10',
            type: 'GET',
            dataType: 'json',
            success: function(reviews) {
                // Clear the loading indicator
                $('#allReviewsList').empty();
                
                if (reviews.length === 0) {
                    $('#allReviewsList').html('<div class="alert alert-light">No reviews available.</div>');
                    return;
                }
                
                // Add each review to the modal
                $.each(reviews, function(i, review) {
                    var userImage = '';
                    if (review.userImageUrl) {
                        userImage = '<img src="' + review.userImageUrl + '" class="rounded-circle me-2" alt="' + review.userName + '" width="40" height="40" style="object-fit: cover;" />';
                    } else {
                        userImage = '<div class="bg-secondary rounded-circle text-white d-flex align-items-center justify-content-center me-2" style="width: 40px; height: 40px;"><i class="fas fa-user"></i></div>';
                    }
                    
                    var reviewHtml = '<div class="list-group-item border-0 border-bottom">' +
                        '<div class="d-flex justify-content-between align-items-center">' +
                        '<div class="d-flex align-items-center">' +
                        userImage +
                        '<div>' +
                        '<h6 class="mb-0">' + review.userName + '</h6>' +
                        '<small class="text-muted">' + new Date(review.reviewDate).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' }) + '</small>' +
                        '</div>' +
                        '</div>' +
                        '<div>';
                    
                    // Add star ratings
                    for (var i = 1; i <= 5; i++) {
                        reviewHtml += '<i class="' + (i <= review.rating ? "fas" : "far") + ' fa-star text-warning small"></i>';
                    }
                    
                    reviewHtml += '</div></div>';
                    
                    // Add comment if available
                    if (review.comment) {
                        reviewHtml += '<p class="mt-2 mb-0">' + review.comment + '</p>';
                    }
                    
                    reviewHtml += '</div>';
                    
                    $('#allReviewsList').append(reviewHtml);
                });
            },
            error: function(xhr, status, error) {
                console.error("AJAX Error:", status, error);
                $('#allReviewsList').html('<div class="alert alert-danger">Failed to load reviews: ' + error + '</div>');
            }
        });
    });
});