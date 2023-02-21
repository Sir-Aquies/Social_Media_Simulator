const LoadMorePosts = {
	Random: 'LoadMoreRandomPosts',
	Top: 'LoadMoreTopPosts',
	Recent: 'LoadMoreRecentPosts',
	Oldest: 'LoadMoreOldPosts'
};

const tabNames = {
	Random: 'random-post-tab',
	Top: 'top-post-tab',
	Recent: 'recent-post-tab',
	Oldest: 'old-post-tab'
};

//Tells the scroll event if posts are loading
let loadingRandomPosts = false;

function SetScrollEventExplore(actionMethodName, startFrom, amountOfRowsPerLoad) {
	if (Number.isNaN(startFrom) || Number.isNaN(amountOfRowsPerLoad) || !IsString(actionMethodName))
		return;

	let startFromRow = parseInt(startFrom);
	const rowsPerLoad = parseInt(amountOfRowsPerLoad);

	window.onscroll = function () {
		if (this.window.scrollY > (mainContainer.clientHeight * (70 / 100)) && !loadingRandomPosts) {
			loadingRandomPosts = true;
			LoadMorePostWhenScrolling(startFromRow, actionMethodName);
			//Increase the starting row after fetching.
			startFromRow += rowsPerLoad;
		}
	}
}

function SwitchToTab(startFromRow, rowsPerLoad) {
	if (Number.isNaN(startFromRow) || Number.isNaN(rowsPerLoad))
		return;

	let tabName = event.target.id;
	let actionMethodName = '';
	let urlTab = '';
	switch (tabName) {
		case tabNames.Random:
			actionMethodName = LoadMorePosts.Random;
			urlTab = 'Random';
			break;
		case tabNames.Top:
			actionMethodName = LoadMorePosts.Top;
			urlTab = 'Top';
			break;
		case tabNames.Recent:
			actionMethodName = LoadMorePosts.Recent;
			urlTab = 'Recent';
			break;
		case tabNames.Oldest:
			actionMethodName = LoadMorePosts.Oldest;
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
			data: { startFromRow: 0, amountOfRows: startFromRow },
			success: function (response) {
				EmptyMainContainer();
				//Add the new posts to the main container.
				AddRangePost(response);

				//Change the action method name of the scrolling event.
				SetScrollEventExplore(actionMethodName, startFromRow, rowsPerLoad);
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
	if (isNaN(startFromRow) || !IsString(actionMethodName))
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
	document.getElementById(tabNames.Random).style.borderBottom = '0';
	document.getElementById(tabNames.Top).style.borderBottom = '0';
	document.getElementById(tabNames.Recent).style.borderBottom = '0';
	document.getElementById(tabNames.Oldest).style.borderBottom = '0';

	if (!IsString(tabName))
		document.getElementById(tabName).style.borderBottom = '3px solid var(--borderColor)';
}

function SelectInicialTab(tabName, startFromRow, rowsPerLoad) {
	if (Number.isNaN(startFromRow) || Number.isNaN(rowsPerLoad))
		return;

	if (!IsString(tabName))
		tabName = 'Random';

	switch (tabName) {
		case 'Top':
			SwitchStyleTab(tabNames.Top);
			SetScrollEventExplore(LoadMorePosts.Top, startFromRow, rowsPerLoad);
			break;
		case 'Recent':
			SwitchStyleTab(tabNames.Recent);
			SetScrollEventExplore(LoadMorePosts.Recent, startFromRow, rowsPerLoad);
			break;
		case 'Oldest':
			SwitchStyleTab(tabNames.Oldest);
			SetScrollEventExplore(LoadMorePosts.Oldest, startFromRow, rowsPerLoad);
			break;
		default:
			SwitchStyleTab(tabNames.Random);
			SetScrollEventExplore(LoadMorePosts.Random, startFromRow, rowsPerLoad);
	}
}

function ReloadUsersInLeftBox(amountOfRows) {
	if (Number.isNaN(amountOfRows))
		return;

	$.ajax(
		{
			type: 'GET',
			url: '/Search/LoadRandomUsers',
			data: { amountOfRows },
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
