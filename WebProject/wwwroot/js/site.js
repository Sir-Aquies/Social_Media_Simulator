const mediaQuery = window.matchMedia('(min-width: 400px)');

mediaQuery.addListener(handleTabletChange);

function handleTabletChange(e) {
    if (e.matches) {
        $("#HeadList").css("margin-left", "auto");
        $("#HeadList").children().css("display", "none")
    } else {
        $("#HeadList").css("margin-left", "0");
    }
};

handleTabletChange(mediaQuery);