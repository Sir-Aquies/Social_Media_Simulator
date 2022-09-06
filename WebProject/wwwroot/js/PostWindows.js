var postclone;

function CreatePostWindow() {
    var postdiv = document.getElementById("PostDiv");
    var tab = Background();
    tab.addEventListener("dblclick", () => { RemoveTab() });
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
    var option = post.children[1];
    option.style.display = "inline";

    //option.style.transform = "translate(-1.5rem, 2rem)";

    document.addEventListener("mousedown", () => {
        option.style.display = "none";
        document.addEventListener("mousedown", () => { });
    });
}

function EditPostWindow(postid, input) {
    input.parentElement.style.display = "none";
    document.addEventListener("mousedown", () => { });

    $.post("Profile/LookforPost", { PostId: postid }, function (data, status) {
        var tab = Background();
        document.body.style.overflow = "hidden";
        const partial = document.getElementById("EditPostDiv");
        partial.style.display = "block";
        tab.appendChild(partial);
        tab.addEventListener("dblclick", () => { RemoveEditPostTab() })

        $('#EditPostDiv').html(data);
    });

    
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

function RemoveEditPostTab() {
    var tab = document.getElementById("BlackBackground");
    const partial = document.getElementById("EditPostDiv");
    partial.style.display = "none";
    document.body.appendChild(partial);
    tab.remove();
    document.body.style.overflow = "auto";
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