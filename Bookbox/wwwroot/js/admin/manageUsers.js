// Manage Users Page JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    initTooltips();
    
    // Handle role changes
    initRoleSelects();

    // Handle view user details buttons
    initViewDetailsButtons();
});

function initTooltips() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(function(tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

function initRoleSelects() {
    document.querySelectorAll('.role-select').forEach(select => {
        // Save original value to detect actual changes
        select.dataset.originalValue = select.value;
        
        select.addEventListener('change', function() {
            const userId = this.getAttribute('data-user-id');
            const username = this.getAttribute('data-username');
            const newRole = this.value;
            const originalRole = this.dataset.originalValue;
            const parentCell = this.closest('.role-cell');
            
            // If no actual change, do nothing
            if (newRole === originalRole) {
                return;
            }
            
            // Create spinner
            const spinner = createSpinner();
            
            // Remember the original content
            const originalHtml = parentCell.innerHTML;
            
            // Show spinner
            parentCell.innerHTML = '';
            parentCell.appendChild(spinner);
            
            // Prepare form data
            const formData = new FormData();
            formData.append('userId', userId);
            formData.append('userType', newRole);

            // Make AJAX request
            fetch('/Admin/UpdateUserRoleAjax', {
                method: 'POST',
                body: formData
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`Server returned ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                // Restore the cell with original select
                parentCell.innerHTML = originalHtml;
                
                // Update the select value and original value data attribute
                const newSelect = parentCell.querySelector('.role-select');
                if (newSelect) {
                    newSelect.value = newRole;
                    newSelect.dataset.originalValue = newRole;
                }
                
                if (data.success) {
                    // Show success modal
                    showSuccessModal(data.message);
                } else {
                    // Show error message
                    showErrorToast(data.message);
                    
                    // Reset the select to original value
                    if (newSelect) {
                        newSelect.value = originalRole;
                    }
                }
            })
            .catch(error => {
                console.error('Error updating role:', error);
                
                // Restore original content
                parentCell.innerHTML = originalHtml;
                
                // Show error message
                showErrorToast('Failed to update user role. Please try again.');
            });
        });
    });
}

function initViewDetailsButtons() {
    document.querySelectorAll('.view-user-btn').forEach(button => {
        button.addEventListener('click', function() {
            const userId = this.getAttribute('data-user-id');
            const modalBody = document.querySelector('#userDetailsModal .modal-body');
            const originalModalContent = modalBody.innerHTML;
            
            // Show loading spinner
            modalBody.innerHTML = createLoadingSpinner();
            
            // Show the modal
            const userModal = new bootstrap.Modal(document.getElementById('userDetailsModal'));
            userModal.show();
            
            // Fetch user details
            fetch(`/Admin/GetUserDetails?userId=${encodeURIComponent(userId)}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Failed to load user details');
                    }
                    return response.json();
                })
                .then(user => {
                    // Reset modal content
                    modalBody.innerHTML = originalModalContent;
                    
                    // Update user details
                    updateUserDetails(user);
                })
                .catch(error => {
                    console.error('Error fetching user details:', error);
                    modalBody.innerHTML = createErrorAlert(error.message);
                });
        });
    });
}

function updateUserDetails(user) {
    // Basic details
    document.getElementById('detailUsername').textContent = user.username;
    document.getElementById('detailFullName').textContent = `${user.firstName} ${user.lastName}`;
    document.getElementById('detailEmail').textContent = user.email;
    document.getElementById('detailContact').textContent = user.contactNo || 'Not provided';
    document.getElementById('detailDOB').textContent = user.dateOfBirth ? formatDate(new Date(user.dateOfBirth)) : 'Not provided';
    
    // Gender (using switch for better readability)
    let genderText;
    switch (user.gender) {
        case 1: genderText = 'Male'; break;
        case 2: genderText = 'Female'; break;
        default: genderText = 'Other';
    }
    document.getElementById('detailGender').textContent = genderText;
    
    // Role
    document.getElementById('detailRole').textContent = user.userType === 2 ? 'Member' : 'Staff';
    document.getElementById('detailRegisteredDate').textContent = formatDate(new Date(user.registeredDate));
    document.getElementById('detailOrders').textContent = user.successfulOrderCount;
    
    // User image
    const imageContainer = document.getElementById('userImageContainer');
    if (user.imageUrlText) {
        imageContainer.innerHTML = `<img src="${user.imageUrlText}" alt="Profile" class="rounded-circle mb-3 shadow" style="width: 150px; height: 150px; object-fit: cover;">`;
    } else {
        imageContainer.innerHTML = `<div class="bg-secondary rounded-circle text-white d-flex align-items-center justify-content-center mx-auto mb-3 shadow" style="width: 150px; height: 150px;">
            <i class="fas fa-user fa-4x"></i>
        </div>`;
    }
}

function showSuccessModal(message) {
    const successModal = new bootstrap.Modal(document.getElementById('successModal'));
    document.getElementById('successMessage').textContent = message;
    successModal.show();
}

function showErrorToast(message) {
    // You could implement a toast notification here
    alert(message);
}

function createSpinner() {
    const spinner = document.createElement('div');
    spinner.className = 'spinner-border spinner-border-sm text-primary';
    spinner.setAttribute('role', 'status');
    
    const span = document.createElement('span');
    span.className = 'visually-hidden';
    span.textContent = 'Loading...';
    
    spinner.appendChild(span);
    return spinner;
}

function createLoadingSpinner() {
    return `<div class="d-flex justify-content-center align-items-center py-5">
        <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
        </div>
    </div>`;
}

function createErrorAlert(message) {
    return `<div class="alert alert-danger">
        <i class="fas fa-exclamation-circle me-2"></i>
        Failed to load user details: ${message}
    </div>`;
}

function formatDate(date) {
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
}