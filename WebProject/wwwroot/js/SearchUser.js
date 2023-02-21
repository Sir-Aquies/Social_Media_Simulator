function SearchUserName(searchBar) {
	let userName = searchBar.value;

	if (!IsString(userName) || userName === "") {
		userTable.innerHTML = "";
		document.getElementById(SearchTabs.RandomUsers).click();
		document.getElementsByClassName('explore-navigation-bar')[0].style.display = 'flex';
		return;
	}

	document.getElementsByClassName('explore-navigation-bar')[0].style.display = 'none';

	$.ajax(
		{
			type: 'GET',
			url: '/Search/LookUsersByUserName',
			data: { userName },
			success: function (data) {
				AddUsersToTable(data, true);
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

const userTable = document.getElementsByClassName('users-table')[0];

function AddUsersToTable(usersHTML, cleanTable = false) {
	//Parsing the usersHTML to object elements returns a document,
	//get the users object elements from the children of the body of the document (index 2).
	const users = new DOMParser().parseFromString(usersHTML, 'text/html').all[2].children;

	if (cleanTable) {
		userTable.innerHTML = "";
	}

	//Store the length
	let length = users.length;

	//Appending users to the table removes them from users, 
	//so always take the first element.
	for (let i = 0; i < length; i++) {
		userTable.appendChild(users[0]);
	}
}

//Name of the tabs in SearchUsers.cshtml
const SearchTabs = {
	RandomUsers: 'random-users',
	TopLikedUsers: 'top-liked-users',
	TopFollowedUsers: 'top-followers-users',
	TopCommentedUsers: 'top-commented-users'
};

//Name of the action methodsm in SearchController.cs
const SearchActionMethods = {
	RandomUsers: 'LoadRandomUsers',
	TopLikedUsers: 'LoadMoreUsersWithMostLikes',
	TopFollowedUsers: 'LoadMoreUsersWithMostFollowers',
	TopCommentedUsers: 'LoadMoreUsersWithMostCommmentsInPosts'
};

//Name of the keys for session storage, items expires after 5 minutes when in other pages.
const SearchSessionKeys = {
	CurrentActionMethod: 'SearchCurrentActionMethod',
	CurrentRow: 'SearchCurrentRow',
	CurrentRowsPerLoad: 'SearchCurrentRowsPerLoad',
	CurrentTab: 'SearchCurrentTabName'
};

//Use to tell if the the window scroll event if users are loading.
let loadingUsers = false;

function SelectActionMethod(tabName) {
	if (typeof tabName !== 'string')
		return;

	switch (tabName) {
		case SearchTabs.RandomUsers:
			return SearchActionMethods.RandomUsers;
			break;
		case SearchTabs.TopLikedUsers:
			return SearchActionMethods.TopLikedUsers;
			break;
		case SearchTabs.TopFollowedUsers:
			return SearchActionMethods.TopFollowedUsers;
			break;
		case SearchTabs.TopCommentedUsers:
			return SearchActionMethods.TopCommentedUsers;
			break;
	}
}

function SetScrollEventSearch(actionMethodName, startFromRow, rowsPerLoad) {
	if (Number.isNaN(startFromRow) || Number.isNaN(rowsPerLoad) || !IsString(actionMethodName))
		return;

	sessionStorage.setItem(SearchSessionKeys.CurrentRow, startFromRow);
	sessionStorage.setItem(SearchSessionKeys.CurrentRowsPerLoad, rowsPerLoad);

	window.onscroll = function () {
		if (!loadingUsers && this.window.scrollY > (userTable.clientHeight - 1000)) {
			loadingUsers = true;

			if (actionMethodName !== SearchActionMethods.RandomUsers) {
				LoadMoreUserWhenScrolling(startFromRow, actionMethodName);

				//Increase the starting row after fetching.
				startFromRow += rowsPerLoad;
				sessionStorage.setItem(SearchSessionKeys.CurrentRow, startFromRow);
			}
		}
	}
}

function SwitchSearchUsersTab(startFromRow, rowsPerLoad, tabId = '') {
	let tabName = event.target.id;

	if (!IsString(tabName) || Number.isNaN(startFromRow) || Number.isNaN(rowsPerLoad))
		return;

	const actionMethod = SelectActionMethod(tabName);

	if (actionMethod === undefined)
		return;

	$.ajax(
		{
			type: 'GET',
			url: `/Search/${actionMethod}`,
			data: { startFromRow: 0, amountOfRows: startFromRow },
			success: function (usersHTML) {
				AddUsersToTable(usersHTML, true);

				//Set the scroll event to fetch more users when scrolling down.
				SetScrollEventSearch(actionMethod, startFromRow, rowsPerLoad);

				//Save the action method and tab name being use.
				sessionStorage.setItem(SearchSessionKeys.CurrentActionMethod, actionMethod);
				sessionStorage.setItem(SearchSessionKeys.CurrentTab, tabName);

				//Change style to the current tab.
				ChangeTabHighlighting(tabName);
				loadingUsers = false;
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function LoadMoreUserWhenScrolling(startFromRow, actionMethod) {
	if (Number.isNaN(startFromRow) || !IsString(actionMethod))
		return;

	$.ajax(
		{
			type: 'GET',
			url: `/Search/${actionMethod}`,
			data: { startFromRow },
			success: function (data) {
				AddUsersToTable(data);

				loadingUsers = false;
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function ReloadPreviousState() {
	//Get the all items from sessionStorage.
	let previousActionMethod = sessionStorage.getItem(SearchSessionKeys.CurrentActionMethod);
	let previousRow = sessionStorage.getItem(SearchSessionKeys.CurrentRow);
	let tabName = sessionStorage.getItem(SearchSessionKeys.CurrentTab);
	let rowsPerLoad = sessionStorage.getItem(SearchSessionKeys.CurrentRowsPerLoad);

	//Validate the items and return false if one is not valid.
	if (!IsString(previousActionMethod) || !IsString(tabName) || Number.isNaN(previousRow) || Number.isNaN(rowsPerLoad))
		return false;

	$.ajax(
		{
			type: 'GET',
			url: `/Search/${previousActionMethod}`,
			data: { startFromRow: 0, amountOfRows: previousRow },
			success: function (usersHTML) {
				AddUsersToTable(usersHTML, true);

				//Set the scroll event to fetch more users when scrolling down.
				SetScrollEventSearch(previousActionMethod, previousRow, rowsPerLoad)
				ChangeTabHighlighting(tabName);

				loadingUsers = false;
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);

	return true;
}

//Use to visualy change the tab the user is on.
function ChangeTabHighlighting(tabName) {
	if (!IsString(tabName))
		return;

	//Remove the highlight of all the tabs.
	for (let tab in SearchTabs) {
		document.getElementById(SearchTabs[tab]).style.borderBottom = '0';
	}

	document.getElementById(tabName).style.borderBottom = '3px solid var(--borderColor)';
}

function ReloadUsers(amountOfRows) {
	if (Number.isNaN(amountOfRows))
		return;

	$.ajax(
		{
			type: 'GET',
			url: '/Search/LoadRandomUsers',
			data: { amountOfRows },
			success: function (data) {
				AddUsersToTable(data, true);
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

window.onload = function () {
	//Clear the timer that deletes all session storage items (GlobalFuntions).
	CancelSessionStorageExpireTimer();

	//Method returns false if it doesn't work
	if (!ReloadPreviousState()) {
		ReloadUsers(10);
	}
};