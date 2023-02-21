//When a post is clicked, it shows the post and all its comments.
function ShowCompletePost(postId) {
	if (Number.isNaN(postId))
		return

	$.ajax(
		{
			type: "GET",
			url: "/Post/ViewPost",
			data: { postId },
			success: function (data) {
				const tab = document.createElement("div");
				tab.id = 'view-black-background';
				tab.className = 'view-post-black-background';
				tab.addEventListener('dblclick', () => { RemoveViewPostTab() });
				document.body.appendChild(tab);

				tab.innerHTML = data;
				document.body.style.overflow = 'hidden';
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function RemoveViewPostTab() {
	document.getElementById('view-black-background').remove();
	document.body.style.overflow = 'auto';
}

//Get CreatePost.cshtml partial view (from LookForCreatePost) and display it in a background.
function CreatePostTab() {
	$.get('/Post/LookForCreatePost', function (data, status) {
		if (status === 'success') {
			const background = Background();
			//Insert the partial view.
			background.insertAdjacentHTML('beforeend', data);
			//focus the textarea element.
			document.getElementById('content-text').focus();
			document.body.style.overflow = 'hidden';
		}
	});
}

//Passes the content and media (if there is) to CreatePost action method who will create the post.
function CreatePost(input) {
	if (input === undefined)
		return;

	const content = input.parentElement.parentElement.children[0].value;
	const [file] = input.parentElement.children[2].files;
	//Get the token for request verification token.
	const token = $('input[name="__RequestVerificationToken"]').val();
	const formData = new FormData();

	//If the user uploaded an image add it to the form.
	if (file) {
		formData.append("Media", file);
	}

	formData.append("Content", content);
	formData.append("__RequestVerificationToken", token);

	$.ajax(
		{
			type: "POST",
			url: "/Post/CreatePost",
			data: formData,
			contentType: false,
			processData: false,
			success: function (data) {
				//Remove the tab created in CreatePostTab. 
				RemoveTab();
				//Insert the data (the new created post) to UserPostContainer.
				AddPostToContainer(data);
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

//Get EditPost.cshtml partial view (from LookforPost) and display it in a background.
function EditPostTab(postId, input) {
	if (Number.isNaN(postId) || input === undefined)
		return;

	//Undisplay the post option button parent element.
	input.parentElement.style.display = "none";

	$.ajax(
		{
			type: "GET",
			url: "/Post/LookforPost",
			data: { PostId: postId },
			success: function (data, status) {
				if (status === 'success') {
					//Create a black background.
					const background = Background();

					//Insert the partial view.
					background.insertAdjacentHTML('beforeend', data);
					//focus the textarea element.
					const textArea = document.getElementById('content-text');
					textArea.focus();
					//Put the cursor at the end of the text.
					textArea.setSelectionRange(textArea.value.length, textArea.value.length);

					//Call function in the case the post had a media (picture) if it did not it will return.
					EditImage();

					document.body.style.overflow = "hidden";
				}
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

//Passes the new content and new media (if there is) to EditPost action method who will edit the post.
function EditPost(input) {
	if (input === undefined)
		return;

	const content = input.parentElement.parentElement.children[0].value;
	const [file] = input.parentElement.children[2].files;
	//Get the token for request verification token.
	const token = $('input[name="__RequestVerificationToken"]').val();
	//Boolean who will say if the media was deleted.
	let deleteMedia = false;
	const formData = new FormData();

	//Get the preview-frame element.
	const oldFrame = document.getElementById("preview-frame");
	//If preview-frame doesn't exist it means the user deleted the media.
	if (!oldFrame) {
		deleteMedia = true;
	}

	if (file) {
		formData.append("Media", file);
	}

	formData.append("DeleteMedia", deleteMedia);
	formData.append("Content", content);
	formData.append("__RequestVerificationToken", token);

	$.ajax(
		{
			type: "POST",
			url: "/Post/EditPost",
			data: formData,
			contentType: false,
			processData: false,
			success: function (data) {
				//Remove the tab created in EditPostTab.
				RemoveTab();
				//Insert the response (data) to the the UserPostContainer.
				UpdatePostFromContainer(data);
				Message('Post successfully edited.');
			},
			error: function (details) {
				RemoveTab();
				Message(details.responseText);
			}
		}
	);
}

//DeletPost passes the id of the post to DeletePost action method who will delete the post and removes the post container from the DOM.
function DeletePost(postId, input) {
	if (Number.isNaN(postId) || input === undefined)
		return;

	//Undisplay the post option button parent element.
	input.parentElement.style.display = "none";
	//Get the token for request verification token.
	const token = $('input[name="__RequestVerificationToken"]').val();

	$.post("/Post/DeletePost", { __RequestVerificationToken: token, PostId: postId }, function (data, status) {
		//the data variable consist of a boolean return from the action method (true for deleted, false for error).
		if (status === "success" && data) {
			//Remove the post container.
			RemovePostFromContainer(postId);
		}
	});
}

//Function that handles when a user likes or dislikes a post.
function LikePost(postId) {
	if (Number.isNaN(postId))
		return;

	$.post("/Post/LikePost", { PostId: postId }, function (data, status) {
		if (status === "success") {
			//the response (data) consist of a string, "+" for like, "-" for dislike and "0" for errors.
			if (data === "+" || data === "-") {
				AlterPostLikes(postId, data);
			}
			else if (data === "0") {
				Message("Somethig went wrong");
			}
		}
	});
}

function LikesTab(postId) {
	if (Number.isNaN(postId))
		return;

	$.get('/Post/PostLikesTab', { postId }, (data, status) => {
		if (status === 'success') {
			const background = Background();

			document.body.style.overflow = 'hidden';

			const tabContainer = document.createElement('div');
			tabContainer.className = 'users-list-tab';
			tabContainer.innerHTML = data;

			background.appendChild(tabContainer);
		}
	});
}

const actionMethods = {
	FollowingPosts: 'LoadMoreFollowingUsersPosts',
	UserPagePosts: 'LoadMorePosts',
	LikedPosts: 'LoadMoreLikedPosts',
	LikedComments: 'LoadMoreLikedComments',
	CommentedPosts: 'LoadMoreCommentedPosts'
};

let loadingPosts = false;

function SelectActionMethod(tabName) {
	if (!IsString(tabName))
		return;

	let methodToUse = '';

	switch (tabName) {
		case '#pageuser-posts':
			methodToUse = actionMethods.UserPagePosts;
			break;
		case '#pageuser-media':
			methodToUse = actionMethods.UserPagePosts;
			break;
		case '#pageuser-likes':
			methodToUse = actionMethods.LikedPosts;
			break;
		case 'liked-comments':
			methodToUse = actionMethods.LikedComments;
			break;
		case '#pageuser-comments':
			methodToUse = actionMethods.CommentedPosts;
			break;
		case '#following-posts':
			methodToUse = actionMethods.FollowingPosts;
			break;
	}

	return methodToUse;
}

function SetScrollEvent(userId, startFromRow, rowsPerLoad, tabName, onlyMediaPosts = false) {
	if (Number.isNaN(startFromRow) || Number.isNaN(rowsPerLoad) || !IsString(tabName) || !IsString(userId))
		return;

	const actionMethod = SelectActionMethod(tabName)

	if (actionMethod === undefined)
		return;

	if (tabName === '#pageuser-media')
		onlyMediaPosts = true;

	window.onscroll = function () {
		if (!loadingPosts && this.window.scrollY > (mainContainer.clientHeight * (70 / 100))) {
			loadingPosts = true;

			if (onlyMediaPosts) {
				LoadMorePostsGlobal(startFromRow, actionMethod, userId, onlyMediaPosts);
			}
			else {
				LoadMorePostsGlobal(startFromRow, actionMethod, userId);
			}

			startFromRow += rowsPerLoad;
		}
	}
}

function LoadMorePostsGlobal(startFromRow, actionMethod, userId = null, onlyMediaPosts = false) {
	if (Number.isNaN(startFromRow) || !IsString(actionMethod))
		return;

	const sendData = { userId, startFromRow };

	if (onlyMediaPosts)
		sendData.onlyMedia = onlyMediaPosts;

	$.ajax(
		{
			type: "GET",
			url: `/User/${actionMethod}`,
			data: sendData,
			success: function (response) {
				if (response) {
					AddRangePost(response);
					loadingPosts = false;
				}
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function SwitchTabLikedPostsAndComments(userId, startingRow, rowsPerLoad) {
	if (!IsString(userId) || Number.isNaN(startingRow) || Number.isNaN(rowsPerLoad))
		return;

	let tabName = event.target.id;
	let actionMethodName = actionMethods.LikedPosts;

	if (tabName === 'switch-comment-tab') {
		actionMethodName = actionMethods.LikedComments;
	}

	$.ajax(
		{
			type: "GET",
			url: `/User/${actionMethodName}`,
			data: { userId, startFromRow: 0 },
			success: function (data) {
				if (data) {
					//Remove all posts from the main container and add the new posts.
					EmptyMainContainer();
					AddRangePost(data);

					//Change the scroll event.
					let postLoader = tabName === 'switch-comment-tab' ? 'liked-comments' : '#pageuser-likes';
					SetScrollEvent(userId, startingRow, rowsPerLoad, postLoader);

					//Visual change to indicate the user which tab is on.
					document.getElementById('switch-post-tab').style.borderBottom = 'none';
					document.getElementById('switch-comment-tab').style.borderBottom = 'none';
					document.getElementById(tabName).style.borderBottom = '3px solid var(--borderColor)';
				}
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

//Observer use to detect if a post is in the viewport.
let postObserver;
window.addEventListener('load', () => {
	postObserver = new IntersectionObserver(DetectPostInViewport, {
		rootMargin: '50px',
		threshold: 0.4
	});

	//The containers varible is in AsyncCRUD.js, 
	//and at this moment only holds the first posts loaded.
	for (let i = 0; i < containers.length; i++) {
		postObserver.observe(containers[i]);
	}
});

function DetectPostInViewport(entries, observer) {
	if (entries === undefined || observer === undefined)
		return;

	//Loop beacause there could be multiple post in the viewport at the same time.
	for (let i = 0; i < entries.length; i++) {
		if (!entries[i].isIntersecting) {
			//entries[i].target.style.backgroundColor = 'white';
			//If the post is out of the viewport clear its timer.
			clearInterval(entries[i].target.updatePostTimer);
			continue;
		}

		const post = entries[i].target;
		//post.style.backgroundColor = 'red';

		post.updatePostTimer = setInterval(() => {
			UpdatePostInfo(entries[i].target.id);
		}, 10000);
	}
}

function UpdatePostInfo(postId) {
	if (Number.isNaN(postId))
		return;

	$.ajax(
		{
			type: 'GET',
			url: '/Post/UpdatePostInfo',
			dataType: 'json',
			data: { postId },
			success: function (postInfo) {
				if (postInfo === undefined)
					return;

				//User arrays because there could be 2 of the same post if view post tab is open.
				const likeButtons = [...document.querySelectorAll(`[data-post-likes-${postId}]`)];
				const commentButtons = [...document.querySelectorAll(`[data-post-comments-${postId}]`)];

				for (let i = 0; i < likeButtons.length; i++) {
					//The first children contains amount of likes.
					const likesSpan = likeButtons[i].children[0];

					if (postInfo.likes > parseInt(likesSpan.innerHTML))
						SparklingLikeEffect(likeButtons[i]);

					likesSpan.innerHTML = postInfo.likes;
				}

				for (let i = 0; i < commentButtons.length; i++) {
					//The first children contains amount of comments.
					const commentSpan = commentButtons[i].children[0];

					if (postInfo.comments > parseInt(commentSpan.innerHTML))
						SparklingLikeEffect(commentButtons[i]);

					commentSpan.innerHTML = postInfo.comments;
				}
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function UnblurImage(cover) {
	if (cover === undefined)
		return;

	const imageContainer = cover.parentElement;
	const image = imageContainer.children[0];
	const button = cover.children[0];

	if (image.style.filter === 'blur(0px)') {
		image.style.filter = 'blur(1rem)';
		button.style.display = 'block';
		cover.style.backgroundColor = 'rgba(0, 0, 0, 0.6)';
	}
	else {
		image.style.filter = 'blur(0px)';
		button.style.display = 'none';
		cover.style.backgroundColor = 'transparent';
	}
}

//This function removes the background along with his tab and restore the overflow.
//The tab can be from CreatePostTab, EditPostTab or CreateCommentTab.
function RemoveTab() {
	document.getElementById("BlackBackground").remove();

	//If the user has the view post tab open do not restore the overflow.
	const viewPostBG = document.getElementById('view-black-background');
	if (!viewPostBG) {
		document.body.style.overflow = "auto";
	}
}

//OptionButton displays and indisplays the options inside the option button from the posts.
function OptionButton(post) {
	if (post === undefined)
		return;

	//Get the post-form.
	const option = post.children[0];

	//If the display is equal to none of undefined, display it.
	if (option.style.display === 'none' || !option.style.display) {
		option.style.display = 'flex';
	}
	else {
		option.style.display = 'none';
	}

	//Add an event to undisplays the options when user clicks outside of the it.
	document.addEventListener("mousedown", function handler() {
		option.style.display = "none";
		this.removeEventListener("mousedown", handler);
	});
}

//This creates and returns a semi transparent black brackground, any element appended will be centered.
function Background() {
	const tab = document.createElement("div");
	tab.id = "BlackBackground";
	tab.className = 'black-background';
	tab.addEventListener("dblclick", () => { RemoveTab() });
	document.body.appendChild(tab);

	const closeTabButton = document.createElement('img');
	closeTabButton.src = '/Icons/close-round-icon.svg';
	closeTabButton.className = 'close-tab-button';
	closeTabButton.onclick = () => { RemoveTab() }

	tab.appendChild(closeTabButton);

	return tab;
}