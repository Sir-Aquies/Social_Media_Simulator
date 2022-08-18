var postclone;

function CreatePostWindow() {
    var postdiv = document.getElementById("PostDiv");
    var tab = Background();
    document.body.style.overflow = "hidden";

    postclone = postdiv.cloneNode(true);
    tab.appendChild(postclone);
    postclone.style.display = "block";

    var collection = postclone.children[0].children;
    collection[2].id = "CurrentImageHolder";
    collection[1].id = "CurrentContentHolder";
    collection[1].children[1].children[2].id = "CurrentFile";
}

function OptionButton(post) {
    //document.body.style.overflow = "hidden";
    var option = post.children[0];
    option.style.display = "inline";

    option.style.transform = "translate(-1.5rem, 2rem)";

    document.addEventListener("mousedown", () => {
        option.style.display = "none";
        document.addEventListener("mousedown", () => { });
        document.body.style.overflow = "auto";
    });
}

//function EditPostWindow(postId) {
//    var editpostdiv = document.getElementById("EditPostDiv");
//    var tab = Background();
//}

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
    tab.addEventListener("dblclick", () => { RemoveTab() })
    document.body.appendChild(tab);
    return tab;
}

function RemoveTab() {
    var tab = document.getElementById("BlackBackground");

    if (postclone) {
        postclone.remove();
    }
    
    if (tab) {
        tab.remove();
    }

    document.body.style.overflow = "auto";
}