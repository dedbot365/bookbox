// Announcement Details JavaScript

document.addEventListener('DOMContentLoaded', function() {
    // Initialize delete modal
    const deleteModal = document.getElementById('deleteAnnouncementModal');
    if (deleteModal) {
        const modal = new bootstrap.Modal(deleteModal, {
            backdrop: 'static',
            keyboard: false
        });
        
        // Set up delete buttons
        const deleteButtons = document.querySelectorAll('.delete-announcement');
        deleteButtons.forEach(button => {
            button.addEventListener('click', function() {
                const id = this.getAttribute('data-id');
                const title = this.getAttribute('data-title');
                
                // You can use these values to customize the modal if needed
                console.log(`Delete announcement requested for: ${id} - ${title}`);
                
                // Show the modal
                modal.show();
            });
        });
    }
    
    // Add animation to metadata cards
    const metadataCards = document.querySelectorAll('.metadata-card');
    metadataCards.forEach((card, index) => {
        // Add staggered animation with delays
        setTimeout(() => {
            card.style.opacity = "1";
            card.style.transform = "translateY(0)";
        }, 100 * index);
    });
});