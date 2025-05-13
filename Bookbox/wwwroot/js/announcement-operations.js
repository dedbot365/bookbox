// Handle announcement operations success messages

$(document).ready(function() {
    // Check for operation success information
    const operation = $('#operationData').data('operation');
    const message = $('#operationData').data('message');
    const title = $('#operationData').data('title');
    
    if (operation) {
        showSuccessModal(operation, message, title);
    }
});

function showSuccessModal(operation, message, title) {
    // Set appropriate icon and color based on operation
    let iconClass, headerClass, headerText;
    
    switch(operation) {
        case 'create':
            iconClass = 'fa-plus-circle';
            headerClass = 'bg-success';
            headerText = 'Announcement Created';
            break;
        case 'edit':
            iconClass = 'fa-edit';
            headerClass = 'bg-primary';
            headerText = 'Announcement Updated';
            break;
        case 'delete':
            iconClass = 'fa-trash';
            headerClass = 'bg-danger';
            headerText = 'Announcement Deleted';
            break;
        default:
            iconClass = 'fa-check-circle';
            headerClass = 'bg-success';
            headerText = 'Operation Successful';
    }
    
    // Update modal content
    $('#successModalLabel').text(headerText);
    $('#successModalHeader').removeClass().addClass(`modal-header text-white ${headerClass}`);
    $('#successModalIcon').removeClass().addClass(`fas ${iconClass} fa-4x mb-3`);
    $('#successModalMessage').text(message);
    
    if (title) {
        $('#successModalTitle').text(title).parent().show();
    } else {
        $('#successModalTitle').parent().hide();
    }
    
    // Show modal
    const successModal = new bootstrap.Modal(document.getElementById('operationSuccessModal'));
    successModal.show();
}