function LoadMoreRandomPosts() {
	$.ajax(
		{
			type: "GET",
			url: "/Explore/LoadMoreRandomPosts",
			success: function (data) {
				AddRangePost(data);
				loadingRandomPosts = false;
			},
			error: function (details) {
				Message(details.responseText)
			}
		}
	);
}

let loadingRandomPosts = false;

function SetScrollEventExplore(startFrom, PostLoader) {
	let startFromRow = parseInt(startFrom);

	window.onscroll = function () {
		if (this.window.scrollY > (mainContainer.clientHeight * (80 / 100)) && !loadingRandomPosts) {
			loadingRandomPosts = true;
			PostLoader(startFromRow);
			startFromRow += 5;
		}
	}
}

function SwitchToTopPosts(inialAmountToLoad) {
	$.ajax(
		{
			type: "GET",
			url: "/Explore/LoadTopPosts",
			data: { startFromRow: 0 },
			success: function (data) {
				EmptyMainContainer();
				AddRangePost(data);

				SetScrollEventExplore(inialAmountToLoad, LoadMoreTopPosts);

				loadingRandomPosts = false;
			},
			error: function (details) {
				Message(details.responseText)
			}
		}
	);
}

function LoadMoreTopPosts(startFromRow) {
	if (!startFromRow)
		return;

	$.ajax(
		{
			type: "GET",
			url: "/Explore/LoadTopPosts",
			data: { startFromRow },
			success: function (data) {
				AddRangePost(data);
				loadingRandomPosts = false;
			},
			error: function (details) {
				Message(details.responseText)
			}
		}
	);
}

function ReloadUsersInLeftBox(amountOfUsers) {
	if (!amountOfUsers)
		return;

	$.ajax(
		{
			type: "GET",
			url: "/User/GetRandomUsersView",
			data: { amountOfUsers },
			success: function (data) {
				const userMightKnowList = document.getElementById('users-might-know');
				userMightKnowList.innerHTML = data;
			},
			error: function (details) {
				Message(details.responseText)
			}
		}
	);
}

window.addEventListener('load', function () {
	SetScrollEventExplore(LoadMoreRandomPosts);
	ReloadUsersInLeftBox(6);
})

