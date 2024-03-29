﻿//Passes the post id and the username to the LookForCreateComment action method and displays its response.
function CreateCommentTab(postId) {
	if (postId !== undefined) {
		$.post("/Post/LookForCreateComment", { PostId: postId }, function (data, status) {
			if (status === "success") {
				//Create a black background.
				const background = Background();
				//Insert CreateComment.cshtml partial view to the background.
				background.insertAdjacentHTML('beforeend', data);
				//focus the textarea element.
				document.getElementById('content-text').focus();
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
function DeleteComment(commentId, postId, input) {
	//Undisplay the post-form div parent.
	input.parentElement.style.display = "none";
	//Get the token for request verification token.
	const token = $('input[name="__RequestVerificationToken"]').val();

	if (commentId && token) {
		$.post("/Post/DeleteComment", { __RequestVerificationToken: token, CommentId: commentId }, function (data, status) {
			//the data variable consist of a boolean return from the action method (true for deleted, false for error).
			if (status === "success" && data) {
				RemoveCommentFromPost(postId, commentId);
			}
		});
	}
}

//This function gets call when a user likes or dislikes a comment.
function LikeComment(commentId) {

	if (commentId != undefined) {
		$.post("/Post/LikeComment", { CommentId: commentId }, function (data, status) {
			if (status === "success") {
				//the response (data) consist of a string, "+" for like, "-" for dislike and "0" for errors.
				if (data === "+" || data === "-") {
					AlterCommentLikes(commentId, data);
				}
				else if (data === "0") {
					Message("Somethig went wrong");
				}
			}
		});
	}
}

function CommentLikesTab(commentId) {
	if (commentId != undefined) {
		$.get('/Post/CommentLikesTab', { commentId: commentId }, (data, status) => {
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