
$(document).ready(function () {
    $('#Submit').change(function () {
        /*$('#myform').submit();*/

        var userId = $("#ModelId").val();
        $.post("/Settings/Appearance", { UserId : userId })
    });
});