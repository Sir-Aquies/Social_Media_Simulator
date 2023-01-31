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

function SetScrollEventExplore() {
	window.addEventListener('scroll', function () {
		if (this.window.scrollY > (mainContainer.clientHeight * (80 / 100)) && !loadingRandomPosts) {
			loadingRandomPosts = true;
			LoadMoreRandomPosts();
		}
	});
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
	SetScrollEventExplore();
	ReloadUsersInLeftBox(6);
})
