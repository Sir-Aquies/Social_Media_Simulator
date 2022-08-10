//var holder = document.getElementById("CurrentHolder");

function ImagePreview(input, holder) {
    const [file] = input.files;
    var contHolder = document.getElementById("CurrentContentHolder");

    if (file) {
        var newImg = document.createElement("img");
        newImg.style.display = "block";
        newImg.style.width = "100%";
        newImg.style.height = "10rem";
        newImg.style.zIndex = "21";
        newImg.src = URL.createObjectURL(file);

        holder.appendChild(newImg);
        holder.style.float = "left";

        contHolder.style.width = "79%";
        contHolder.style.marginRight = "1%";
        contHolder.style.float = "left";
    }
}
