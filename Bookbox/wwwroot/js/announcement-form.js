// Announcement Form JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Elements
    const previewBtn = document.getElementById('previewAnnouncementBtn');
    const form = document.getElementById('announcementForm');
    const titleInput = document.getElementById('Title');
    const contentInput = document.getElementById('announcementContent');
    const startDateInput = document.getElementById('StartDate');
    const endDateInput = document.getElementById('EndDate');
    const isActiveSwitch = document.getElementById('isActiveSwitch');
    
    // Preview modal elements
    const previewTitle = document.getElementById('previewTitle');
    const previewContent = document.getElementById('previewContent');
    const previewStartDate = document.getElementById('previewStartDate');
    const previewEndDate = document.getElementById('previewEndDate');
    const previewStatus = document.getElementById('previewStatus');
    
    // Initialize the modal
    const previewModal = new bootstrap.Modal(document.getElementById('previewModal'));
    
    // Format date for display
    function formatDate(dateString) {
        if (!dateString) return 'Not set';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    }
    
    // Determine announcement status based on dates and active state
    function getAnnouncementStatus() {
        const isActive = isActiveSwitch.checked;
        if (!isActive) {
            return { text: 'Inactive', class: 'bg-danger' };
        }
        
        const now = new Date();
        const startDate = startDateInput.value ? new Date(startDateInput.value) : null;
        const endDate = endDateInput.value ? new Date(endDateInput.value) : null;
        
        if (startDate && startDate > now) {
            return { text: 'Scheduled', class: 'bg-warning' };
        } else if (!endDate || endDate >= now) {
            return { text: 'Active', class: 'bg-success' };
        } else {
            return { text: 'Expired', class: 'bg-secondary' };
        }
    }
    
    // Show preview modal with current form data
    function showPreview() {
        // Set title (use placeholder if empty)
        previewTitle.textContent = titleInput.value || 'Announcement Title';
        
        // Set content (use placeholder if empty)
        previewContent.innerHTML = contentInput.value || 'Announcement content will appear here.';
        
        // Set dates
        previewStartDate.textContent = startDateInput.value ? formatDate(startDateInput.value) : 'Today';
        previewEndDate.textContent = endDateInput.value ? formatDate(endDateInput.value) : 'No end date';
        
        // Set status
        const status = getAnnouncementStatus();
        previewStatus.textContent = status.text;
        previewStatus.className = `badge ${status.class}`;
        
        // Show the modal
        previewModal.show();
    }
    
    // Event listeners
    previewBtn.addEventListener('click', showPreview);
    
    // Set default values if not already set
    if (!startDateInput.value) {
        const today = new Date();
        startDateInput.valueAsDate = today;
    }
    
    // Add a week for end date if not set
    if (!endDateInput.value) {
        const nextWeek = new Date();
        nextWeek.setDate(nextWeek.getDate() + 7);
        // Format the date as YYYY-MM-DD for the input
        const year = nextWeek.getFullYear();
        const month = String(nextWeek.getMonth() + 1).padStart(2, '0');
        const day = String(nextWeek.getDate()).padStart(2, '0');
        endDateInput.value = `${year}-${month}-${day}`;
    }
    
    // Ensure end date is after start date when changed
    startDateInput.addEventListener('change', function() {
        if (endDateInput.value && startDateInput.value > endDateInput.value) {
            // Calculate new end date (start date + 7 days)
            const newEndDate = new Date(startDateInput.value);
            newEndDate.setDate(newEndDate.getDate() + 7);
            
            // Format the date as YYYY-MM-DD
            const year = newEndDate.getFullYear();
            const month = String(newEndDate.getMonth() + 1).padStart(2, '0');
            const day = String(newEndDate.getDate()).padStart(2, '0');
            
            endDateInput.value = `${year}-${month}-${day}`;
        }
    });
});