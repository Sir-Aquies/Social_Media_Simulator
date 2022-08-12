var postdiv = document.getElementById("PostDiv");
var tab = document.createElement("div");
var postclone;

function CreatePostWindow() { 
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
    tab.addEventListener("click", () => { RemoveTab() })
    document.body.appendChild(tab);

    postclone = postdiv.cloneNode(true);
    tab.appendChild(postclone);
    postclone.style.display = "block";

    var collection = postclone.children[0].children;
    collection[2].id = "CurrentImageHolder";
    collection[1].id = "CurrentContentHolder"
    collection[1].children[1].children[2].id = "CurrentFile";
}

function RemoveTab() {
    postclone.remove();
    tab.remove();   
}