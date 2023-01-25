//Get CreatePost.cshtml partial view (from LookForCreatePost) and display it in a background.
function CreatePostTab() {
	$.get("/Post/LookForCreatePost", function (data, status) {
		if (status === "success") {
			const background = Background();
			//Insert the partial view.
			background.innerHTML = data;
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
					background.innerHTML = data;

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
function LikePost(postId, button) {
	//Span element with the likes amount.
	const likesAmount = button.children[0];
	//SVG element
	const likeSVG = button.children[1];

	if (postId != undefined) {
		$.post("/Post/LikePost", { PostId: postId }, function (data, status) {
			if (status === "success") {
				//the response (data) consist of a string, "+" for like, "-" for dislike and "0" for errors.
				if (data === "+") {
					//Increase the amount of likes.
					let likes = parseInt(likesAmount.innerHTML);
					likesAmount.innerHTML = ++likes;

					//Add a class to the SVG so it looks liked and change title.
					likeSVG.className.baseVal = 'like-button liked';
					button.style.fontWeight = "bold";
					button.title = "Dislike post";
				}
				else if (data === "-") {
					//Decrease the amount of likes.
					let likes = parseInt(likesAmount.innerHTML);
					likesAmount.innerHTML = --likes;

					//Remove the liked class from the SVG so it looks unliked and change title.
					likeSVG.className.baseVal = 'like-button';
					button.style.fontWeight = "normal";
					button.title = "Like post";
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
		$.get('/Post/PostLikesTab', { postId: postId }, (data, status) => {
			if (status === 'success') {
				const background = Background();

				document.body.style.overflow = 'hidden';

				background.innerHTML = data;
			}
		});
	}
}

function KeepLoadingPosts(userId, from, to) {
	if (userId === undefined || from === undefined || to === undefined)
		return;

	$.ajax(
		{
			type: "GET",
			url: "/User/KeepLoadingPosts",
			data: {userId, from, to},
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

function LoadMorePostsMedia(userId, from, to) {
	if (userId === undefined || from === undefined || to === undefined)
		return;

	onlyMedia = true;

	$.ajax(
		{
			type: "GET",
			url: "/User/KeepLoadingPosts",
			data: { userId, from, to, onlyMedia },
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

function LoadMorePostsLikes(userId, from, to) {
	if (userId === undefined || from === undefined || to === undefined)
		return;

	onlyMedia = true;

	$.ajax(
		{
			type: "GET",
			url: "/User/KeepLoadingPosts",
			data: { userId, from, to, onlyMedia },
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

//This function removes the background along with his tab and restore the overflow.
//The tab can be from CreatePostTab, EditPostTab or CreateCommentTab.
function RemoveTab() {
	document.getElementById("BlackBackground").remove();
	document.body.style.overflow = "auto";
}

//OptionButton displays and indisplays the options inside the option button from the posts.
function OptionButton(post) {
	//Get the post-form.
	const option = post.children[1];

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

	return tab;
}