var holder = document.getElementById("ImageHolder");

function ImagePreview(input) {
    const [file] = input.files;

    if (file) {
        var newImg = document.createElement("img");
        newImg.style.display = "block";
        newImg.style.width = "100%";
        newImg.style.height = "10rem";
        newImg.style.zIndex = "21";
        newImg.style.backgroundColor = "red";
        newImg.src = URL.createObjectURL(file);

        holder.appendChild(newImg);       
    }
}
