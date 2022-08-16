var postclone;

function CreatePostWindow() {
    var postdiv = document.getElementById("PostDiv");
    var tab = Background();

    postclone = postdiv.cloneNode(true);
    tab.appendChild(postclone);
    postclone.style.display = "block";

    var collection = postclone.children[0].children;
    collection[2].id = "CurrentImageHolder";
    collection[1].id = "CurrentContentHolder";
    collection[1].children[1].children[2].id = "CurrentFile";
}

function OptionButton(postId) {
    var option = document.getElementById(`Post${ postId }`);
    option.style.display = "inline";
    option.style.left = event.clientX + "px";
    option.style.top = event.clientY + "px";

}

//function EditPostWindow(postId) {
//    var editpostdiv = document.getElementById("EditPostDiv");
//    var tab = Background();
//}

function Background() {
    var tab = document.createElement("div");
    tab.style.position = "absolute";
    tab.style.width = "100%";
    tab.style.height = "100%";
    tab.style.left = "0px";
    tab.style.top = "0px";
    tab.style.backgroundColor = "rgba(0, 0, 0, 0.65)";
    tab.style.zIndex = "10";
    tab.style.display = "flex";
    tab.style.justifyContent = "center";
    tab.style.alignItems = "center";
    tab.addEventListener("click", () => { RemoveTab(tab) })
    document.body.appendChild(tab);
    document.body.style.overflow = "hidden";
    return tab;
}

function RemoveTab(tab) {
    postclone.remove();
    tab.remove();
    document.body.style.overflow = "auto";
}