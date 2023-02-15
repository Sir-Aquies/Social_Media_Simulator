const LoadMoreRandomPostsMethodName = 'LoadMoreRandomPosts';
const LoadMoreTopPostsMethodName = 'LoadMoreTopPosts';
const LoadMoreRecentPostsMethodName = 'LoadMoreRecentPosts';
const LoadMoreOldPostsMethodName = 'LoadMoreOldPosts';

//Tells the scroll event if posts are loading
let loadingRandomPosts = false;

function SetScrollEventExplore(startFrom, actionMethodName) {
	let startFromRow = parseInt(startFrom);

	window.onscroll = function () {
		if (this.window.scrollY > (mainContainer.clientHeight * (70 / 100)) && !loadingRandomPosts) {
			loadingRandomPosts = true;
			LoadMorePostWhenScrolling(startFromRow, actionMethodName);
			//New posts are loaded 5 by 5.
			startFromRow += 5;
		}
	}
}

function SwitchToTab(inicialAmountToLoad) {
	if (!inicialAmountToLoad)
		return;
	let tabName = event.target.id;
	let actionMethodName = '';
	let urlTab = '';
	switch (tabName) {
		case 'random-post-tab':
			actionMethodName = LoadMoreRandomPostsMethodName;
			urlTab = 'Random';
			break;
		case 'top-post-tab':
			actionMethodName = LoadMoreTopPostsMethodName;
			urlTab = 'Top';
			break;
		case 'recent-post-tab':
			actionMethodName = LoadMoreRecentPostsMethodName;
			urlTab = 'Recent';
			break;
		case 'old-post-tab':
			actionMethodName = LoadMoreOldPostsMethodName;
			urlTab = 'Oldest';
			break;
	}

	if (actionMethodName.length === 0)
		return;

	//Change the url.
	window.history.replaceState({}, '', urlTab);

	$.ajax(
		{
			type: "GET",
			url: `/Explore/${actionMethodName}`,
			data: { startFromRow: 0, amountOfRows: inicialAmountToLoad },
			success: function (response) {
				EmptyMainContainer();
				//Add the new posts to the main container.
				AddRangePost(response);

				//Change the action method name of the scrolling event.
				SetScrollEventExplore(inicialAmountToLoad, actionMethodName);
				SwitchStyleTab(tabName);

				//Set loading to false just in case is true.
				loadingRandomPosts = false;
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function LoadMorePostWhenScrolling(startFromRow, actionMethodName) {
	if (!startFromRow || !actionMethodName)
		return;

	$.ajax(
		{
			type: "GET",
			url: `/Explore/${actionMethodName}`,
			data: { startFromRow },
			success: function (data) {
				AddRangePost(data);
				loadingRandomPosts = false;
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function SwitchStyleTab(tabName) {
	document.getElementById('random-post-tab').style.borderBottom = '0';
	document.getElementById('top-post-tab').style.borderBottom = '0';
	document.getElementById('recent-post-tab').style.borderBottom = '0';
	document.getElementById('old-post-tab').style.borderBottom = '0';

	if (tabName)
		document.getElementById(tabName).style.borderBottom = '3px solid var(--borderColor)';
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
				Message(details.responseText);
			}
		}
	);
}
