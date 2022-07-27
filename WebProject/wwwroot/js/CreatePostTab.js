var postdiv = document.getElementById("PostDiv");
var tab = document.createElement("div");

function CreatePostWindow() { 
    tab.style.position = "absolute";
    tab.style.width = window.innerWidth + "px";
    tab.style.height = window.innerHeight + "px";
    tab.style.left = "0px";
    tab.style.top = "0px";
    tab.style.backgroundColor = "rgba(0, 0, 0, 0.65)";
    tab.style.zIndex = "10";
    tab.style.display = "flex";
    tab.style.justifyContent = "center";
    tab.style.alignItems = "center";
    tab.addEventListener("click", () => { RemoveTab() })
    document.body.appendChild(tab);

    tab.appendChild(postdiv);
    postdiv.style.display = "block";
}

function RemoveTab() {
    tab.remove();
}