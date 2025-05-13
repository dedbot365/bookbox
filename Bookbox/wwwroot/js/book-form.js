// JavaScript for book creation and editing forms

$(document).ready(function () {
    // Enhance label clicks for better UX
    $(".cover-upload-container").click(function() {
        $("#ImageFile").click();
    });
    
    // Preview image when selected
    $("#ImageFile").change(function () {
        if (this.files && this.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $("#coverPreview").html('<img src="' + e.target.result + '" class="img-fluid" />');
            };
            reader.readAsDataURL(this.files[0]);
        }
    });
    
    // Add animation for form sections
    $(".form-group").each(function(index) {
        $(this).css({
            'opacity': 0,
            'transform': 'translateY(20px)'
        }).delay(index * 50).animate({
            'opacity': 1,
            'transform': 'translateY(0px)'
        }, 500);
    });
});