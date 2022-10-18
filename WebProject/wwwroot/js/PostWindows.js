function CreatePostWindow() {
	$.post("/Post/LookForCreatePost", function (data, status) {
		if (status === "success") {
			const postdiv = document.getElementById("PostDiv");
			const tab = Background();
			tab.addEventListener("dblclick", () => { RemoveTab() });
			document.body.style.overflow = "hidden";
			postdiv.style.display = "block";
			tab.appendChild(postdiv);

			$('#PostDiv').html(data);
		}
	});
}

function CreatePost(input) {
	const content = input.parentElement.parentElement.children[0].value;
	const [file] = input.parentElement.children[2].files;
	var formData = new FormData();

	if (file) {
		formData.append("Media", file);
	}

	formData.append("Content", content);

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
	const postdiv = document.getElementById("PostDiv");
	const tab = document.getElementById("BlackBackground");

	postdiv.style.display = "none";
	document.body.appendChild(postdiv);

	if (tab) {
		tab.remove();
	}

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
			document.body.style.overflow = "hidden";
			const partial = document.getElementById("EditPostDiv");
			partial.style.display = "block";
			tab.appendChild(partial);
			tab.addEventListener("dblclick", () => { RemoveEditPostTab() })

			$('#EditPostDiv').html(data);
		}
	});
}

function EditPost(input) {
	const content = input.parentElement.parentElement.children[0].value;
	const [file] = input.parentElement.children[2].files;
	var deleteMedia = false;
	var formData = new FormData();

	const oldFrame = document.getElementById("ImageFrame");

	if (!oldFrame) {
		deleteMedia = true;
	}

	if (file) {
		formData.append("Media", file);
	}

	formData.append("DeleteMedia", deleteMedia);
	formData.append("Content", content);

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
	const tab = document.getElementById("BlackBackground");
	const partial = document.getElementById("EditPostDiv");
	partial.style.display = "none";
	document.body.appendChild(partial);
	tab.remove();
	document.body.style.overflow = "auto";
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