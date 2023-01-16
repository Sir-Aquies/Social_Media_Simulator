//Passes the post id and the username to the LookForCreateComment action method and displays its response.
function CreateCommentTab(postId) {
	if (postId !== undefined) {
		$.post("/Post/LookForCreateComment", { PostId: postId }, function (data, status) {
			if (status === "success") {
				//Create a black background.
				const background = Background();
				//Add a delete event.
				background.addEventListener("dblclick", () => { RemoveTab() });
				//Insert CreatePost.cshtml partial view to the background.
				background.innerHTML = data;
				//Hide the overflow of the document.
				document.body.style.overflow = "hidden";
			}
		});
	}
}

//CreateComment gets the content from the CreatePost.cshtml and passes it to CreateComment action method in PostController.
function CreateComment(input) {
	//The comment's content.
	const content = input.parentElement.parentElement.children[0].value;
	//Get the token for request verification token (there are one for every tab, not sure if its necessary).
	const token = $('input[name="__RequestVerificationToken"]').val();

	if (content && token) {
		$.post("/Post/CreateComment", { __RequestVerificationToken: token, Content: content }, function (data, status) {
			if (status === "success") {
				//Remove the background.
				RemoveTab();
				//Add the commento to the respective post.
				AddCommentToPost(data);
			}
		});
	}
}

//DeletComment passes the id of the comment to DeleteComment action method and removes the comment in the DOM.
function DeleteComment(commentId, postId, userName, input) {
	//Undisplay the post-form div parent.
	input.parentElement.style.display = "none";
	//Get the token for request verification token.
	const token = $('input[name="__RequestVerificationToken"]').val();

	if (commentId && token) {
		$.post("/Post/DeleteComment", { __RequestVerificationToken: token, CommentId: commentId }, function (data, status) {
			//the data variable consist of a boolean return from the action method (true for deleted, false for error).
			if (status === "success" && data) {
				//Decrease the amount of comments traveling from the document tree (will change in the future).
				//const commentAmount = input.parentElement.parentElement.parentElement.parentElement.parentElement.children[2].children[1].children[0];
				//commentAmount.innerHTML = parseInt(commentAmount.innerHTML) - 1;
				//Remove the comment container (post-comments).
				RemoveCommentFromPost(postId, userName, commentId);
			}
		});
	}
}

//This function gets call when a user likes or dislikes a comment.
function LikeComment(commentId, button) {
	//span element that conatins the amount of likes.
	const likesAmount = button.children[0];
	//SVG element contain in the like button.
	const likeSVG = button.children[1];

	if (commentId != undefined) {
		$.post("/Post/LikeComment", { CommentId: commentId }, function (data, status) {
			if (status === "success") {
				//the response (data) consist of a string, "+" for like, "-" for dislike and "0" for errors.
				if (data === "+") {
					//Increase the amount of likes.
					let likes = parseInt(likesAmount.innerHTML);
					likesAmount.innerHTML = ++likes;

					//Add a class to the SVG so it looks liked.
					likeSVG.className.baseVal = 'like-button liked';
					button.style.fontWeight = "bold";
					button.title = "Dislike comment";
				}
				else if (data === "-") {
					//decrease the amount of likes.
					let likes = parseInt(likesAmount.innerHTML);
					likesAmount.innerHTML = --likes;

					//Remove the class from the SVG so it looks unliked
					likeSVG.className.baseVal = 'like-button';
					button.style.fontWeight = "normal";
					button.title = "Like comment";
				}

			}
		});
	}
}