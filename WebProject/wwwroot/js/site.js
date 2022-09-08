const mediaQuery = window.matchMedia('(max-width: 564px)');

mediaQuery.addListener(handleTabletChange);

function handleTabletChange(e) {
    if (e.matches) {
        $("#TabList").css("display", "none");
        $("#HeadList").click(function () {
            if ($("#TabList").css("display") == "none") {
                $("#TabList").css("display", "flex");
            } else {
                $("#TabList").css("display", "none");
            }
        });

        $(document).mousedown(function () {
            $("#TabList").css("display", "none");
            $(document).unbind()
        });
    } else {
        $("#TabList").css("display", "flex");
        $("#HeadList").unbind();
        $(document).unbind();
    }
};

handleTabletChange(mediaQuery);