function LoadUsersWithMostLikes() {
	$.ajax(
		{
			type: "GET",
			url: "/User/UsersWithMostLikesView",
			success: function (data) {
				const users = new DOMParser().parseFromString(data, 'text/html').all[2].children;
				let length = users.length;
				const container = document.getElementById('user-most-likes');

				for (let i = 0; i < length; i++) {
					container.appendChild(users[0]);
				}
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function LoadUsersWithMostPosts() {
	$.ajax(
		{
			type: "GET",
			url: "/User/UsersWithMostPostsView",
			success: function (data) {
				const users = new DOMParser().parseFromString(data, 'text/html').all[2].children;
				let length = users.length;
				const container = document.getElementById('user-most-posts');

				for (let i = 0; i < length; i++) {
					container.appendChild(users[0]);
				}
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function Follow(userId) {
	if (!userId)
		return;
	const button = event.target;

	$.ajax(
		{
			type: "POST",
			url: "/User/Follow",
			data: { userId },
			success: function (data) {
				if (data === '+') {
					button.innerHTML = 'Unfollow';
					AlterFollowers(data);
				}

				if (data === '-') {
					button.innerHTML = 'Follow';
					AlterFollowers(data);
				}

				if (!data) {
					Message('An error has ocurred, your follow was not saved.');
				}
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function FollowersUsersTab(userId) {
	if (!userId)
		return;

	$.ajax(
		{
			type: "POST",
			url: "/User/FollowersUsersTab",
			data: { userId },
			success: function (data) {
				if (!data)
					return;

				const background = Background();

				document.body.style.overflow = 'hidden';

				const tabContainer = document.createElement('div');
				tabContainer.className = 'users-list-tab';
				tabContainer.innerHTML = data;

				background.appendChild(tabContainer);
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function FollowingUsersTab(userId) {
	if (!userId)
		return;

	$.ajax(
		{
			type: "POST",
			url: "/User/FollowingUsersTab",
			data: { userId },
			success: function (data) {
				if (!data)
					return;

				const background = Background();

				document.body.style.overflow = 'hidden';

				const tabContainer = document.createElement('div');
				tabContainer.className = 'users-list-tab';
				tabContainer.innerHTML = data;

				background.appendChild(tabContainer);
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function AlterFollowers(action) {
	const userFollowers = [...document.querySelectorAll('[data-user-followers]')];

	for (let i = 0; i < userFollowers.length; i++) {
		if (action === '+') {
			let amountOfFollowers = parseInt(userFollowers[i].innerHTML);
			userFollowers[i].innerHTML = ++amountOfFollowers;
		}

		if (action === '-') {
			let amountOfFollowers = parseInt(userFollowers[i].innerHTML);
			userFollowers[i].innerHTML = --amountOfFollowers;
		}
	}
}

function UpdateUserStats(userId) {
	if (!userId)
		return;

	$.ajax(
		{
			type: "GET",
			url: "/User/UpdateUserStats",
			data: { userId },
			success: function (data) {

			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}



window.addEventListener('load', function () {
	LoadUsersWithMostLikes();
	LoadUsersWithMostPosts()
})