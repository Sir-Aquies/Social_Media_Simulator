const mediaQuery = window.matchMedia('(max-width: 524px)');
mediaQuery.addListener(HandleTabletChange);

//HandleTabletChange makes the top bar resposive to mobile devices.
function HandleTabletChange(e) {
	//Exectes if the width of the screes is lower than 524px.
	if (e.matches) {
		//Undisplay TabList.
		$("#TabList").css("display", "none");

		//Add an event to HeadList that displays TabList if its display is none.
		$("#HeadList").click(function () {
			if ($("#TabList").css("display") == "none") {
				$("#TabList").css("display", "flex");

				//Add an event to the document that when the user clicks anywhere else undisplays TabList.
				$(document).mousedown(function () {
					$("#TabList").css("display", "none");
					$(document).unbind();
				});
			} else {
				//If TabList is already displayed, undisplay it and remove any events in the document.
				$("#TabList").css("display", "none");
				$(document).unbind();
			}
		});
	} else {
		//If the width of the screen is bigger, display Tablist and remove any events in document and HeadList.
		$("#TabList").css("display", "flex");
		$("#HeadList").unbind();
		$(document).unbind();
	}
}

//First check, because the event doesn't when it starts.
HandleTabletChange(mediaQuery);

//This function toggles the display-tab element.
function ToggleTab() {
	const displayTab = document.getElementById("display-tab");
	const pic = document.getElementById("UserMobilePic");

	if (!displayTab.className.includes('move-left')) {
		ShowTab();

		//Add an event in the document so when the user clicks outside of the tab if hides it.
		document.addEventListener("mousedown", HideTab);
	}
	else {
		HideTab();
	}

	//Show display-tab to the left by adding move-left class.
	//Move left has one declaration left: 0; which overides left: -100% in display-tab class. 
	function ShowTab() {
		displayTab.className = 'display-tab move-left';
		pic.style.boxShadow = "0 0px 5px 3px rgba(0, 150, 255, 0.4)";
		document.body.style.overflow = 'hidden';
	}

	//Hide display-tab by removing move-left class and remove the document 
	function HideTab() {
		displayTab.className = 'display-tab';
		pic.style.boxShadow = "none";
		document.body.style.overflow = 'auto';

		document.removeEventListener("mousedown", HideTab);
	}
}
