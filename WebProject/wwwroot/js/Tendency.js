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
				if (data) {
					if (data === '+') {
						button.innerHTML = 'Unfollow';
					}
					else if (data === '-') {
						button.innerHTML = 'Follow';
					}
					else {
						Message('An error has ocurred, your follow was not saved.');
					}
				}
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