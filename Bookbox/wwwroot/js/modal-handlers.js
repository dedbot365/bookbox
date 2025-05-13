/**
 * Modal Handlers - Common functionality for modals across the application
 * This file handles cart and wishlist modals with timers
 */

/**
 * Initialize a modal with timer functionality
 * @param {string} flag - Flag indicating if modal should be shown (e.g. "True")
 * @param {string} title - Title text to display in modal
 * @param {string} modalTitleId - DOM ID for the element where title should be displayed
 * @param {string} modalId - DOM ID of the modal element
 * @param {string} timerBarId - DOM ID of the timer progress bar element
 * @param {string} timerTextId - DOM ID of the timer text element
 */
function initModalWithTimer(flag, title, modalTitleId, modalId, timerBarId, timerTextId) {
    if (flag === 'True') {
        // Set the title if available
        if (title) {
            document.getElementById(modalTitleId).textContent = title;
        }
        
        // Show the modal
        const modal = new bootstrap.Modal(document.getElementById(modalId));
        modal.show();
        
        // Set up timer
        let timeLeft = 10;
        const timerBar = document.getElementById(timerBarId);
        const timerText = document.getElementById(timerTextId);
        
        const timer = setInterval(() => {
            timeLeft--;
            const percentage = (timeLeft / 10) * 100;
            timerBar.style.width = percentage + '%';
            timerText.textContent = timeLeft + 's';
            
            if (timeLeft <= 0) {
                clearInterval(timer);
                modal.hide();
            }
        }, 1000);
        
        // Clear timer if modal is closed manually
        document.getElementById(modalId).addEventListener('hidden.bs.modal', function() {
            clearInterval(timer);
        });
    }
}