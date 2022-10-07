$(document).ready(function () {
    $('#ShowImages').change(function () {
        $.post("/Settings/ShowImagesToggle");
    });
});