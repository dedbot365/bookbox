$(document).ready(function() {
    // Animation for profile elements
    animateProfile();
    
    // Add interactivity to profile information blocks
    setupInfoBlocks();
    
    // Setup tooltips
    initTooltips();
    
    // Special welcome for admin and staff - DISABLED
    // if ($('.admin-profile-header').length) {
    //     setTimeout(function() {
    //         showAdminNotification("Welcome, Administrator", "You have full access to all system features.");
    //     }, 1000);
    // } else if ($('.staff-profile-header').length) {
    //     setTimeout(function() {
    //         showAdminNotification("Welcome, Staff Member", "You have access to book inventory and orders management.");
    //     }, 1000);
    // }
    
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Click to copy email address
    $("#email-value").click(function() {
        var email = $(this).text().trim();
        navigator.clipboard.writeText(email).then(function() {
            showNotification("Email copied to clipboard!", "success");
        }, function() {
            showNotification("Failed to copy email. Please try again.", "error");
        });
    });
    
    // Check if user is admin or staff and show welcome notification - DISABLED
    // if ($(".admin-badge").length > 0) {
    //     setTimeout(function() {
    //         showNotification("Welcome, Administrator. You have full system access.", "admin");
    //     }, 1000);
    // } else if ($(".staff-badge").length > 0) {
    //     setTimeout(function() {
    //         showNotification("Welcome, Staff Member. You have elevated access.", "staff");
    //     }, 1000);
    // }
    
    // Animation for elements
    $(".profile-section").each(function(index) {
        var $this = $(this);
        setTimeout(function() {
            $this.addClass("show");
        }, 200 * index);
    });
});

// Rest of the functions remain unchanged
function animateProfile() {
    // Animate profile card appearance with staggered timing
    const profileCard = $('.profile-card');
    profileCard.addClass('fade-in-up');
    
    // Animate profile sections with a staggered delay
    $('.profile-section').each(function(i) {
        const section = $(this);
        setTimeout(function() {
            section.addClass('fade-in-up');
        }, 100 + (i * 100));
    });
}

function setupInfoBlocks() {
    // Add hover effect to info blocks
    $('.profile-info-value').hover(
        function() {
            $(this).css('transform', 'translateX(5px)');
        },
        function() {
            $(this).css('transform', 'translateX(0)');
        }
    );
    
    // Add click to copy functionality for email
    $('#email-value').click(function() {
        const email = $(this).text().trim();
        
        // Create a temporary input to copy text
        const tempInput = $('<input>');
        $('body').append(tempInput);
        tempInput.val(email).select();
        document.execCommand('copy');
        tempInput.remove();
        
        // Show copied notification
        const originalText = $(this).text();
        $(this).text('Email copied!');
        setTimeout(() => {
            $(this).text(originalText);
        }, 1500);
    });
}

function initTooltips() {
    // Initialize Bootstrap tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Functions below are kept for future use if needed
function showAdminNotification(title, message) {
    // Function kept but not used
}

function showNotification(message, type) {
    // This function is still used for email copy notification
    // Remove any existing notifications
    $(".custom-notification").remove();
    
    // Create the notification
    var icon = "fas fa-info-circle";
    var bgColor = "#4D4490"; // Default purple
    
    if (type === "success") {
        icon = "fas fa-check-circle";
        bgColor = "#4D4490"; // Purple for success
    } else if (type === "error") {
        icon = "fas fa-exclamation-triangle";
        bgColor = "#4D4490"; // Purple for error
    } else if (type === "admin") {
        icon = "fas fa-user-cog";
        bgColor = "#3a2f7d"; // Dark purple for admin
    } else if (type === "staff") {
        icon = "fas fa-user-tie";
        bgColor = "#6a55c7"; // Light purple for staff
    }
    
    var notification = `<div class="custom-notification" style="background-color: ${bgColor}">
                          <i class="${icon}"></i>
                          <span>${message}</span>
                       </div>`;
    
    // Append to body and animate
    $("body").append(notification);
    setTimeout(function() {
        $(".custom-notification").addClass("show");
        
        // Auto hide after 5 seconds
        setTimeout(function() {
            $(".custom-notification").removeClass("show");
            setTimeout(function() {
                $(".custom-notification").remove();
            }, 300);
        }, 5000);
    }, 100);
}
