var emptyinput = document.createElement("input");
emptyinput.type = "file";

function ImagePreview(input, imageHolder, contHolder) {
	const [file] = input.files;

	if (file) {
		var oldFrame = document.getElementById("ImageFrame");
		if (oldFrame) {
			oldFrame.children[1].src = URL.createObjectURL(file);
			return;
		}

		var frame = CreateFrame(file);
		frame.children[0].onclick = DeleteImagePreview;
		imageHolder.appendChild(frame);

		imageHolder.style.float = "left";
		contHolder.style.width = "79%";
		contHolder.style.marginRight = "1%";
		contHolder.style.float = "left";
	}
}

function DeleteImagePreview() {
	const imageFrame = document.getElementById("ImageFrame");
	const contHolder = document.getElementById("ContentHolder");
	const imageHolder = document.getElementById('ImageHolder');
	const file = document.getElementById('File');

	//Remove the frame along with his childs
	imageFrame.remove();

	//Remove the file from the file input
	file.files = emptyinput.files;

	//Set everything like it was before
	imageHolder.style.float = "none";
	contHolder.style.width = "100%";
	contHolder.style.marginRight = "0";
	contHolder.style.float = "none";
}

function EditImagePreview(input, holder) {
	const [file] = input.files;
	const contHolder = document.getElementById("EditContentHolder");

	if (file) {
		const oldFrame = document.getElementById("ImageFrame");
		if (oldFrame) {
			oldFrame.children[1].src = URL.createObjectURL(file);
			return;
		}

		var frame = CreateFrame(file);
		frame.children[0].onclick = DeleteEditImagePreview;
		holder.appendChild(frame);

		holder.style.float = "left";
		contHolder.style.width = "79%";
		contHolder.style.marginRight = "1%";
		contHolder.style.float = "left";
	}
}

function DeleteEditImagePreview() {
	const imageFrame = document.getElementById("ImageFrame");
	const contHolder = document.getElementById("EditContentHolder");
	const imageHolder = document.getElementById('EditImageHolder');
	const file = document.getElementById('EditFile');

	imageFrame.remove();

	file.files = emptyinput.files;

	imageHolder.style.float = "none";
	contHolder.style.width = "100%";
	contHolder.style.marginRight = "0";
	contHolder.style.float = "none";
}

function EditImage(picture) {
	var frame = CreateFrame();
	frame.children[0].onclick = DeleteEditImagePreview;
	frame.children[1].src = picture;

	const contHolder = document.getElementById("EditContentHolder");
	const imageHolder = document.getElementById("EditImageHolder");

	imageHolder.appendChild(frame);

	imageHolder.style.float = "left";
	contHolder.style.width = "79%";
	contHolder.style.marginRight = "1%";
	contHolder.style.float = "left";
}

function CreateFrame(picture) {
	var div = document.createElement("div");
	div.id = "ImageFrame";
	div.style.width = "100%";
	div.style.height = "auto";
	div.style.padding = "0";
	div.style.margin = "0";

	var button = document.createElement("span");
	button.style.width = "1rem";
	button.style.height = "1rem";
	button.style.borderRadius = "50%";
	button.style.float = "right";
	button.style.marginBottom = "3px";
	button.style.cursor = "pointer";
	button.style.backgroundColor = "red";
	button.style.backgroundImage = "url(../button.png)";
	button.style.backgroundSize = "contain";
	button.style.backgroundPosition = "center";
	button.style.backgroundRepeat = "no-repeat"

	var newImg = document.createElement("img");
	newImg.style.display = "block";
	newImg.style.width = "100%";
	newImg.style.height = "auto";
	if (picture) {
		newImg.src = URL.createObjectURL(picture);
	}

	div.appendChild(button);
	div.appendChild(newImg);

	return div;
};

/*EditProfileView*/
function SettingImagePreview(input) {
	const [file] = input.files;

	if (file) {
		var frame = document.getElementById("SettingsFrame");

		if (!frame) {
			return;
		}

		frame.src = URL.createObjectURL(file);
	}
};

function FakeCall() {
	document.getElementById("Settingsfile").click();
};