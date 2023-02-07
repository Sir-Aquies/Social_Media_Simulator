//When a post is clicked, it shows the post and all its comments.
function ShowCompletePost(postId) {
	if (!postId)
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
				tab.addEventListener("dblclick", () => { RemoveViewPostTab() });
				document.body.appendChild(tab);

				tab.innerHTML = data;
				document.body.style.overflow = "hidden";
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
	$.get("/Post/LookForCreatePost", function (data, status) {
		if (status === "success") {
			const background = Background();
			//Insert the partial view.
			background.insertAdjacentHTML('beforeend', data);
			//focus the textarea element.
			document.getElementById('content-text').focus();
			document.body.style.overflow = "hidden";
		}
	});
}

//Passes the content and media (if there is) to CreatePost action method who will create the post.
function CreatePost(input) {
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
	//Undisplay the post option button parent element.
	input.parentElement.style.display = "none";

	if (!postId)
		return;

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
	//Undisplay the post option button parent element.
	input.parentElement.style.display = "none";
	//Get the token for request verification token.
	const token = $('input[name="__RequestVerificationToken"]').val();

	if (postId != undefined) {
		$.post("/Post/DeletePost", { __RequestVerificationToken: token, PostId: postId }, function (data, status) {
			//the data variable consist of a boolean return from the action method (true for deleted, false for error).
			if (status === "success" && data) {
				//Remove the post container.
				RemovePostFromContainer(postId);
			}
		});
	}
}

//Function that handles when a user likes or dislikes a post.
function LikePost(postId) {

	if (postId != undefined) {
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
}

function LikesTab(postId) {
	if (postId != undefined) {
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
}

function LoadMorePosts(userId, startFromRow, amountOfRows) {
	if (!userId || !startFromRow || !amountOfRows)
		return;

	$.ajax(
		{
			type: "GET",
			url: "/User/LoadMorePosts",
			data: { userId, startFromRow, amountOfRows },
			success: function (data) {
				if (data) {
					AddRangePost(data);
					loadingPosts = false;
				}
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function LoadMorePostsMedia(userId, startFromRow, amountOfRows) {
	if (!userId || !startFromRow || !amountOfRows)
		return;

	onlyMedia = true;

	$.ajax(
		{
			type: "GET",
			url: "/User/LoadMorePosts",
			data: { userId, startFromRow, amountOfRows, onlyMedia },
			success: function (data) {
				if (data) {
					AddRangePost(data);
					loadingPosts = false;
				}
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function LoadMorePostsLikes(userId, startFromRow, amountOfRows) {
	if (!userId || !startFromRow || !amountOfRows)
		return;

	$.ajax(
		{
			type: "GET",
			url: "/User/LoadMorePostsLikes",
			data: { userId, startFromRow, amountOfRows },
			success: function (data) {
				if (data) {
					AddRangePost(data);
					loadingPosts = false;
				}
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function SwitchToPosts(userId, inicialAmountToLoad) {
	if (!userId || !inicialAmountToLoad)
		return;

	$.ajax(
		{
			type: "GET",
			url: "/User/LoadMorePostsLikes",
			data: { userId, startFromRow: 0, amountOfRows: inicialAmountToLoad },
			success: function (data) {
				if (data) {
					//Remove all posts from the main container and add the liked posts.
					EmptyMainContainer();
					AddRangePost(data);

					//Change event to load liked post.
					SetScrollEvent(userId, inicialAmountToLoad, LoadMorePostsLikes);

					//Visual change to indicate the user witch is on.
					document.getElementById('switch-post-tab').style.borderBottom = '3px solid var(--BorderColor)';
					document.getElementById('switch-comment-tab').style.borderBottom = 'none';
				}
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function SwitchToComments(userId, inicialAmountToLoad) {
	if (!userId || !inicialAmountToLoad)
		return;

	$.ajax(
		{
			type: "GET",
			url: "/User/LoadMoreLikedComments",
			data: { userId, startFromRow: 0, amountOfRows: inicialAmountToLoad },
			success: function (data) {
				if (data) {
					//Remove all the posts from the main container and add the liked comments's posts.
					EmptyMainContainer();
					AddRangePost(data);

					//Change event to load liked comments along with its post.
					SetScrollEvent(userId, inicialAmountToLoad, LoadMoreLikedComments);

					//Visual change to indicate the user witch is on.
					document.getElementById('switch-post-tab').style.borderBottom = 'none';
					document.getElementById('switch-comment-tab').style.borderBottom = '3px solid var(--BorderColor)';
				}
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function LoadMoreLikedComments(userId, startFromRow, amountOfRows) {
	if (!userId || !startFromRow || !amountOfRows)
		return;

	$.ajax(
		{
			type: "GET",
			url: "/User/LoadMoreLikedComments",
			data: { userId, startFromRow, amountOfRows },
			success: function (data) {
				if (data) {
					AddRangePost(data);
					loadingPosts = false;
				}
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function LoadMorePostsComments(userId, startFromRow, amountOfRows) {
	if (!userId || !startFromRow || !amountOfRows)
		return;

	$.ajax(
		{
			type: "GET",
			url: "/User/LoadMorePostsComments",
			data: { userId, startFromRow, amountOfRows },
			success: function (data) {
				if (data) {
					AddRangePost(data);
					loadingPosts = false;
				}
			},
			error: function (details) {
				Message(details.responseText);
			}
		}
	);
}

function UnblurImage(cover) {
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

function Message(message) {
	const messageContainer = document.createElement('aside');
	messageContainer.className = 'alert-message';

	messageContainer.innerHTML = message;

	const deleteButton = document.createElement('button');
	deleteButton.className = 'close-button';
	deleteButton.onclick = function() {
		this.parentElement.style.display = 'none';
	}
	messageContainer.appendChild(deleteButton);

	document.getElementById('main-header').appendChild(messageContainer);
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