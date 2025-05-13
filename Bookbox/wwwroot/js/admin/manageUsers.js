// Manage Users Page JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    initTooltips();
    
    // Handle view user details buttons
    initViewDetailsButtons();
    
    // Handle edit user buttons
    initEditUserButtons();
    
    // Handle save role changes
    initSaveRoleChanges();
});

function initTooltips() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(function(tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
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

function initEditUserButtons() {
    document.querySelectorAll('.edit-user-btn').forEach(button => {
        button.addEventListener('click', function() {
            const userId = this.getAttribute('data-user-id');
            const username = this.getAttribute('data-username');
            const fullName = this.getAttribute('data-full-name');
            const currentRole = this.getAttribute('data-current-role');
            const imageUrl = this.getAttribute('data-image-url');
            
            // Set values in the edit modal
            document.getElementById('editUserId').value = userId;
            document.getElementById('editUserName').textContent = fullName;
            document.getElementById('editUserUsername').textContent = '@' + username;
            document.getElementById('editUserRole').value = currentRole;
            
            // Update user image
            const imageContainer = document.getElementById('editUserImageContainer');
            if (imageUrl) {
                imageContainer.innerHTML = `<div class="avatar-circle"><img src="${imageUrl}" alt="${fullName}" class="shadow"></div>`;
            } else {
                imageContainer.innerHTML = `<div class="avatar-circle bg-secondary text-white d-flex align-items-center justify-content-center">
                    <i class="fas fa-user fa-3x"></i>
                </div>`;
            }
            
            // Show modal
            const editModal = new bootstrap.Modal(document.getElementById('editUserModal'));
            editModal.show();
        });
    });
}

function initSaveRoleChanges() {
    document.getElementById('saveRoleChanges').addEventListener('click', function() {
        const userId = document.getElementById('editUserId').value;
        const userRoleSelect = document.getElementById('editUserRole');
        const newRole = userRoleSelect.value;
        const userName = document.getElementById('editUserName').textContent;
        
        // Show loading state
        const saveBtn = this;
        const originalText = saveBtn.innerHTML;
        saveBtn.disabled = true;
        saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> Saving...';
        
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
            // Reset button state
            saveBtn.disabled = false;
            saveBtn.innerHTML = originalText;
            
            // Close edit modal
            const editModal = bootstrap.Modal.getInstance(document.getElementById('editUserModal'));
            editModal.hide();
            
            if (data.success) {
                // Update the role badge in the table
                updateUserRoleDisplay(userId, newRole);
                
                // Show success modal
                showSuccessModal(data.message);
            } else {
                // Show error message
                showErrorToast(data.message);
            }
        })
        .catch(error => {
            console.error('Error updating role:', error);
            
            // Reset button state
            saveBtn.disabled = false;
            saveBtn.innerHTML = originalText;
            
            // Show error message
            showErrorToast('Failed to update user role. Please try again.');
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

function updateUserRoleDisplay(userId, roleValue) {
    // Find the row with this user
    const row = document.querySelector(`[data-user-id="${userId}"]`).closest('tr');
    if (!row) return;
    
    // Find the role display cell
    const roleCell = row.querySelector('.role-display');
    if (!roleCell) return;
    
    // Update badge class and text
    const roleName = roleValue === "2" ? "Member" : "Staff";
    const badgeClass = roleValue === "2" ? "info" : "primary";
    
    roleCell.innerHTML = `<span class="badge bg-${badgeClass}">${roleName}</span>`;
    
    // Also update the data attribute on the edit button
    const editButton = row.querySelector('.edit-user-btn');
    if (editButton) {
        editButton.setAttribute('data-current-role', roleValue);
    }
}

function showSuccessModal(message) {
    const successModal = new bootstrap.Modal(document.getElementById('successModal'));
    document.getElementById('successMessage').textContent = message;
    successModal.show();
}

function showErrorToast(message) {
    // Create a Bootstrap toast
    const toastContainer = document.createElement('div');
    toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
    toastContainer.style.zIndex = '9999';
    
    const toastEl = document.createElement('div');
    toastEl.className = 'toast align-items-center text-white bg-danger border-0';
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');
    
    toastEl.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                <i class="fas fa-exclamation-circle me-2"></i>
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;
    
    toastContainer.appendChild(toastEl);
    document.body.appendChild(toastContainer);
    
    const toast = new bootstrap.Toast(toastEl, {
        delay: 5000
    });
    
    toast.show();
    
    // Remove from DOM after hiding
    toastEl.addEventListener('hidden.bs.toast', function () {
        document.body.removeChild(toastContainer);
    });
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