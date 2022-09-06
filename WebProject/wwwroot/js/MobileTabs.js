
function ToggleTab() {
    const leftbox = document.getElementById("DisplayTab");

    if (leftbox.slide == "on") {
        leftbox.slide = "off";
        leftbox.style.display = "none";
        document.addEventListener("mouseup", () => { })
    }
    else {
        leftbox.slide = "on";
        leftbox.style.display = "block";
        document.addEventListener("mouseup", () => {
            leftbox.slide = "off";
            leftbox.style.display = "none";
            document.addEventListener("mouseup", () => {})
        })
    }
    
}