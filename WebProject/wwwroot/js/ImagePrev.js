//Input element with a type file use to remove the file/files from others input file type.
//Yes, I didn't find a better way to do that.
const emptyinput = document.createElement("input");
emptyinput.type = "file";

//ImagePreview adds an image preview when the user uploads one.
//It gets call in CreatePost.cshtml by an file input element on an onchange event.
//The first parameter is the input element itself and the second is a div that holds the preview frame.
function ImagePreview(input, frameHolder) {
	const [file] = input.files;

	if (file) {
		//Check if a preview frame already exist by getting it by its id.
		const oldFrame = document.getElementById("preview-frame");

		//If it does we just change its children img src to the new file.
		if (oldFrame) {
			oldFrame.children[1].src = URL.createObjectURL(file);
			return;
		}

		//Create a frame for the file (there can only be one).
		var frame = CreateFrame();
		//Add the delete event to the delete button of the frame.
		frame.children[0].onclick = DeleteImagePreview;
		//Set the image to the img element of frame.
		frame.children[1].src = URL.createObjectURL(file);
		//Append the frame to the holder.
		frameHolder.appendChild(frame);
	}
}

//DeleteImagePreview deletes the frame and adjust the element in the form.
function DeleteImagePreview() {
	//Remove the frame along with his childs
	document.getElementById("preview-frame").remove();

	//Remove the file from the file input
	document.getElementById('File').files = emptyinput.files;
}

//This function create a frame if the post model that the user wants to edit already have a media (an image).
//It always gets call from PostWindow.js EditPostWindow function.
function EditImage() {
	//Get the old image from an img element loaded in an if statement in Editpost.cshtml
	const oldImage = document.getElementById("SRC");

	//If the img element doesn't exist just return.
	if (!oldImage) {
		return;
	}

	//Create the frame, add the delete event and load the old image.
	const frame = CreateFrame();
	frame.children[0].onclick = DeleteImagePreview;
	frame.children[1].src = oldImage.src;

	//Add the frame to frameHolder and display it.
	const frameHolder = document.getElementById("frame-holder");
	frameHolder.appendChild(frame);

	oldImage.remove();
}

//Creates and returns the frame that will contain the image preview (file) and the delete button.
function CreateFrame() {
	var frame = document.createElement("div");
	frame.id = "preview-frame";
	frame.className = 'preview-frame';

	var deleteButton = document.createElement("span");
	deleteButton.className = 'delete-frame-button';

	var imagePreview = document.createElement("img");
	imagePreview.className = 'image-preview';

	frame.appendChild(deleteButton);
	frame.appendChild(imagePreview);

	return frame;
}