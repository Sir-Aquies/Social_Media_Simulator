var emptyinput = document.createElement("input");
emptyinput.type = "file";

function ImagePreview(input, holder) {
    const [file] = input.files;
    var contHolder = document.getElementById("CurrentContentHolder");

    if (file) {
        var oldImg = document.getElementById("NewImg");
        if (oldImg) {
            oldImg.src = URL.createObjectURL(file);
            return;
        }

        var newImg = document.createElement("img");
        newImg.style.display = "block";
        newImg.style.width = "100%";
        newImg.style.height = "auto";
        newImg.id = "NewImg";
        newImg.src = URL.createObjectURL(file);

        var x = CreateCancelButton();
        x.appendChild(newImg);

        holder.appendChild(x);
        holder.style.float = "left";

        contHolder.style.width = "79%";
        contHolder.style.marginRight = "1%";
        contHolder.style.float = "left";
    }
}

function CreateCancelButton() {
    var div = document.createElement("div");
    div.style.width = "100%";
    div.style.height = "auto";
    div.style.padding = "0";
    div.style.margin = "0";

    var button = document.createElement("span");
    button.style.width = "1rem";
    button.style.height = "1rem";
    button.style.borderRadius = "50%";
    button.style.float = "right";
    button.style.margin = "0"
    button.style.marginBottom = "3px";
    button.style.cursor = "pointer";
    button.style.backgroundColor = "red";
    button.style.backgroundImage = "url(../button.png)";
    button.style.backgroundSize = "contain";
    button.style.backgroundPosition = "center";
    button.style.backgroundRepeat = "no-repeat"
    button.onclick = DeleteImagePreview;
    div.appendChild(button);

    return div;
}

function DeleteImagePreview() {
    var Img = document.getElementById("NewImg");
    var contHolder = document.getElementById("CurrentContentHolder");
    var holder = document.getElementById('CurrentImageHolder');
    var File = document.getElementById('CurrentFile');

    Img.remove();
    this.remove();

    File.files = emptyinput.files;

    holder.style.float = "none";

    contHolder.style.width = "100%";
    contHolder.style.marginRight = "0";
    contHolder.style.float = "none";
}

function EditImage(img) {
    var div = CreateCancelButton();
    div.appendChild(img);

    var ContHolder = document.getElementById("EditContentHolder");
    var Holder = document.getElementById("EditImageHolder");


    holder.appendChild(div);
    holder.style.float = "left";

    contHolder.style.width = "79%";
    contHolder.style.marginRight = "1%";
    contHolder.style.float = "left";
}

function SettingImagePreview(input) {
    const [file] = input.files;

    if (file) {
        var frame = document.getElementById("SettingsFrame");

        if (!frame) {
            return;
        }

        frame.src = URL.createObjectURL(file);
    }
}

function FakeCall() {
    document.getElementById("Settingsfile").click();
}