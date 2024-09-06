// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('#videoUrlForm').on('submit', function (e) {
        e.preventDefault(); // Prevent default form submission

        var videoUrl = $('#Txt_Url').val();

        // Show confirmation alert using SweetAlert
        Swal.fire({
            icon: 'success',
            title: 'URL Submitted',
            text: 'You entered: ' + videoUrl,
        }).then((result) => {
            if (result.isConfirmed) {
                // If confirmed, submit the form
                $(this).off('submit').submit();
            }
        });
    });
});
