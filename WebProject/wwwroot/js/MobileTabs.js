const mediaQuery = window.matchMedia('(max-width: 564px)');
mediaQuery.addListener(handleTabletChange);

function handleTabletChange(e) {
	if (e.matches) {
		$("#TabList").css("display", "none");
		
		$("#HeadList").click(function () {
			if ($("#TabList").css("display") == "none") {
				$("#TabList").css("display", "flex");
				$(document).mousedown(function () {
					$("#TabList").css("display", "none");
					$(document).unbind()
				});
			} else {
				$("#TabList").css("display", "none");
			}
		});
	} else {
		$("#TabList").css("display", "flex");
		$("#HeadList").unbind();
		$(document).unbind();
	}
};

handleTabletChange(mediaQuery);

function ToggleTab() {
	const leftbox = document.getElementById("DisplayTab");
	const pic = document.getElementById("UserMobilePic");

	if (leftbox.slide == "off") {
		Show();

		document.addEventListener("mousedown", function () {
			Hide();
		});
	}
	else {
		Hide();
	}

	function Show() {
		$("#DisplayTab").addClass("left");
		pic.style.boxShadow = "0 0px 5px 3px rgba(0, 150, 255, 0.4)";
		leftbox.slide = "on";
	}

	function Hide() {
		$("#DisplayTab").removeClass("left");
		pic.style.boxShadow = "none";
		leftbox.slide = "off";

		document.addEventListener("mousedown", () => { })
	}
};

ToggleTab();
