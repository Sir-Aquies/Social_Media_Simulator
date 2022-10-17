function CreateCommentTab(postId) {
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

function LikePost(postId, button) {
	likesAmount = button.children[0];

	if (postId != undefined) {
		$.post("/Post/LikePost", { PostId: postId }, function (data, status) {
			if (status === "success") {
				if (data === "+") {
					let likes = parseInt(likesAmount.innerHTML);
					likesAmount.innerHTML = ++likes;
					button.style.fontWeight = "bold";
				}
				else if (data === "-") {
					let likes = parseInt(likesAmount.innerHTML);
					likesAmount.innerHTML = --likes;
					button.style.fontWeight = "normal";
				}
				
			}
		});
	}
}

function LikeComment(commentId, button) {
	likesAmount = button.children[0];

	if (commentId != undefined) {
		$.post("/Post/LikeComment", { CommentId: commentId }, function (data, status) {
			if (status === "success") {
				if (data === "+") {
					let likes = parseInt(likesAmount.innerHTML);
					likesAmount.innerHTML = ++likes;
					button.style.fontWeight = "bold";
				}
				else if (data === "-") {
					let likes = parseInt(likesAmount.innerHTML);
					likesAmount.innerHTML = --likes;
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