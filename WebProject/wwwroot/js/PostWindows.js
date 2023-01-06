function CreatePostWindow() {
	$.post("/Post/LookForCreatePost", function (data, status) {
		if (status === "success") {
			const tab = Background();
			tab.addEventListener("dblclick", () => { RemoveTab() });
			tab.innerHTML = data;

			document.body.style.overflow = "hidden";
		}
	});
}

function CreatePost(input) {
	const content = input.parentElement.parentElement.children[0].value;
	const [file] = input.parentElement.children[2].files;
	const token = $('input[name="__RequestVerificationToken"]').val();
	var formData = new FormData();

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
				RemoveTab();
				$("#UserPostContainer").html(data);
			}
		}
	);
}

function RemoveTab() {
	document.getElementById("BlackBackground").remove();
	document.body.style.overflow = "auto";
}

function OptionButton(post) {
	var option = post.children[1];
	option.style.display = "flex";

	document.addEventListener("mousedown", () => {
		option.style.display = "none";
		this.removeEventListener("mousedown", arguments.callee);
	});
}

function EditPostWindow(postid, input) {
	input.parentElement.style.display = "none";

	$.post("/Post/LookforPost", { PostId: postid }, function (data, status) {
		if (status === "success") {
			const tab = Background();
			tab.addEventListener("dblclick", () => { RemoveEditPostTab() });
			tab.innerHTML = data;
			EditImage();
			document.body.style.overflow = "hidden";
		}
	});
}

function EditPost(input) {
	const content = input.parentElement.parentElement.children[0].value;
	const token = $('input[name="__RequestVerificationToken"]').val();
	const [file] = input.parentElement.children[2].files;
	var deleteMedia = false;
	var formData = new FormData();

	const oldFrame = document.getElementById("preview-frame");

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
				RemoveEditPostTab();
				$("#UserPostContainer").html(data);
			}
		}
	);
}

function RemoveEditPostTab() {
	document.getElementById("BlackBackground").remove();
	document.body.style.overflow = "auto";
}

function LikePost(postId, button) {
	const likesAmount = button.children[0];
	const likeSVG = button.children[1];

	if (postId != undefined) {
		$.post("/Post/LikePost", { PostId: postId }, function (data, status) {
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

function DeletePost(postId, input) {
	input.parentElement.style.display = "none";
	const token = $('input[name="__RequestVerificationToken"]').val();

	if (postId != undefined) {
		$.post("/Post/DeletePost", { __RequestVerificationToken: token, PostId: postId }, function (data, status) {
			if (status === "success") {
				if (data) {
					const postContainer = input.parentElement.parentElement.parentElement.parentElement;
					postContainer.remove();
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