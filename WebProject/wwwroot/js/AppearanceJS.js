
$(document).ready(function () {
    $('#myform input[type="checkbox"]').change(function () {
        $('#myform').submit();
    });
});