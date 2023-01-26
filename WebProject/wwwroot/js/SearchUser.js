function SearchUserName(searchBar) {
	let userName = searchBar.value;

	if (userName === "") {
		userTable.innerHTML = "";
		ReloadUsers(10);
	}
	
	if (!userName)
		return

	$.ajax(
		{
			type: "GET",
			url: "/User/LookUsersByUserName",
			data: { userName },
			success: function (data) {
				PutUsersInTheList(data);
			},
			error: function (details) {

			}
		}
	);
}

const userTable = document.getElementsByClassName('users-table')[0];

function PutUsersInTheList(usersString) {
	const users = new DOMParser().parseFromString(usersString, 'text/html').all[2].children;

	if (userTable.children.length > 0) {
		userTable.innerHTML = "";
	}

	let length = users.length;

	for (let i = 0; i < length; i++) {
		userTable.appendChild(users[0]);
	}
}

function ReloadUsers(amount) {
	if (!amount)
		return;

	$.ajax(
		{
			type: "GET",
			url: "/User/GetRandomUsersView",
			data: { amount },
			success: function (data) {
				PutUsersInTheList(data);
			},
			error: function (details) {

			}
		}
	);
}


window.onload = function () {
	ReloadUsers(10);
};