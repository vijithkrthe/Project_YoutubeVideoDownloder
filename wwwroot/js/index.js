$(document).ready(function () {
    $("#Btn_Show").on("click", function () {
        var url = $("#Txt_Url").val().trim();
        var mediaType = $("input[name='mediaType']:checked").val();

        if (!url || !mediaType) {
            Swal.fire("Error", "Please enter a valid URL and select media type.", "error");
            return;
        }

        var youtubeUrlPattern = /^(https?:\/\/)?(www\.)?youtube\.com\/watch\?v=[\w-]{11}$/;
        if (!youtubeUrlPattern.test(url)) {
            Swal.fire("Error", "Please enter a valid YouTube URL.", "error");
            return;
        }
        Swal.fire({
            title: 'Loading...',
            text: 'Fetching media details...',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        $.ajax({
            url: '/Home/GetMediaDetails',
            type: 'GET',
            data: { url: url, mediaType: mediaType },
            success: function (data) {
                Swal.close(); 
                if (data.thumbnailUrl) {
                    $("#thumbnail").attr("src", data.thumbnailUrl).show();
                } else {
                    $("#thumbnail").hide();
                }

                var qualitySelect = $("#qualitySelect");
                if (mediaType === "video") {
                    if (data.qualities.length > 0) {
                        qualitySelect.empty();
                        $.each(data.qualities, function (index, quality) {
                            qualitySelect.append($('<option>', {
                                value: quality,
                                text: quality
                            }));
                        });
                        qualitySelect.show();
                        $("#qualitySection").show();
                    } else {
                        qualitySelect.hide();
                        $("#qualitySection").hide();
                        Swal.fire("Info", "No video qualities available.", "info");
                    }
                } else {
                    $("#qualitySelect").hide();
                    $("#qualitySection").hide();
                }

                $("#downloadSection").show();
            },
            error: function (xhr, status, error) {
                Swal.close(); // Close the loading indicator
                console.error("AJAX Error:", status, error);
                Swal.fire("Error", "Failed to fetch media details. Please try again later.", "error");
            }
        });
    });

    // Trigger file download
    $("#Btn_Download").on("click", function () {
        var url = $("#Txt_Url").val().trim();
        var quality = $("#qualitySelect").val();
        var mediaType = $("input[name='mediaType']:checked").val();

        if (!url || !mediaType || (mediaType === "video" && !quality)) {
            Swal.fire("Error", "Please enter a valid URL, select media type, and choose quality if necessary.", "error");
            return;
        }
        $.ajax({
            url: '/Home/DownloadMedia',
            method: 'POST',
            data: {
                url: url,
                quality: quality,
                mediaType: mediaType
            },
            success: function (response) {
                
                Swal.fire("Success", `The video "${response.validTitle}" has been downloaded successfully!`, "success");
            },
            error: function () {
                Swal.fire("Error", "An error occurred while downloading the video.", "error");
            }
        });
    });
    $("#Btn_Refresh").on("click", function () {
        location.reload();
    });
});
