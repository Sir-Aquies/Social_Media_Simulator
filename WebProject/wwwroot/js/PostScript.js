﻿function CreateCommentTab(postId) {
	const arr = window.location.href.split('/');
	const userName = arr[arr.length - 1];

	if (postId !== undefined && userName) {
		$.post("/Post/LookForCreateComment", { PostId: postId, Username: userName }, function (data, status) {
			if (status === "success") {
				const commentdiv = document.getElementById("CommentDiv");
				const tab = Background();
				tab.addEventListener("dblclick", () => { RemoveCreateCommentTab() });
				document.body.style.overflow = "hidden";
				commentdiv.style.display = "block";
				tab.appendChild(commentdiv);

				$('#CommentDiv').html(data);
			}
		});
	}
}

function CreateComment(input) {
	const content = input.parentElement.parentElement.children[0].value;
	const token = $('input[name="__RequestVerificationToken"]').val();

	if (content) {
		$.post("/Post/CreateComment", { __RequestVerificationToken: token, Content: content }, function (data, status) {
			if (status === "success") {
				RemoveCreateCommentTab();
				$("#UserPostContainer").html(data);
			}
		});
	}
}

function RemoveCreateCommentTab() {
	const commentdiv = document.getElementById("CommentDiv");
	const tab = document.getElementById("BlackBackground");

	commentdiv.style.display = "none";
	document.body.appendChild(commentdiv);

	if (tab) {
		tab.remove();
	}

	document.body.style.overflow = "auto";
}

function DeleteComment(commentId, input) {
	input.parentElement.style.display = "none";
	const token = $('input[name="__RequestVerificationToken"]').val();

	if (commentId != undefined) {
		$.post("/Post/DeleteComment", { __RequestVerificationToken: token, CommentId: commentId }, function (data, status) {
			if (status === "success") {
				if (data) {
					const commentAmount = input.parentElement.parentElement.parentElement.parentElement.parentElement.children[2].children[1].children[0];
					commentAmount.innerHTML = parseInt(commentAmount.innerHTML) - 1;
					const postContainer = input.parentElement.parentElement.parentElement.parentElement;
					postContainer.remove();
				}
			}
		});
	}
}

function LikeComment(commentId, button) {
	likesAmount = button.children[0];
	const likeSVG = button.children[1];

	if (commentId != undefined) {
		$.post("/Post/LikeComment", { CommentId: commentId }, function (data, status) {
			if (status === "success") {
				if (data === "+") {
					let likes = parseInt(likesAmount.innerHTML);
					likesAmount.innerHTML = ++likes;

					likeSVG.className.baseVal = 'like-button liked';
					button.style.fontWeight = "bold";
				}
				else if (data === "-") {
					let likes = parseInt(likesAmount.innerHTML);
					likesAmount.innerHTML = --likes;

					likeSVG.className.baseVal = 'like-button';
					button.style.fontWeight = "normal";
				}

			}
		});
	}
}

function Background() {
	var tab = document.createElement("div");
	tab.id = "BlackBackground";
	tab.style.position = "fixed";
	tab.style.width = "100%";
	tab.style.height = "100vh";
	tab.style.left = "0px";
	tab.style.top = "0px";
	tab.style.backgroundColor = "rgba(0, 0, 0, 0.65)";
	tab.style.zIndex = "10";
	tab.style.display = "flex";
	tab.style.justifyContent = "center";
	tab.style.alignItems = "center";
	document.body.appendChild(tab);
	return tab;
}