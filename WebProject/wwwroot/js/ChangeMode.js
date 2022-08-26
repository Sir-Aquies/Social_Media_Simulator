const savedTheme = localStorage.getItem('Mode');

if (savedTheme) {
    SetMode(savedTheme);
}


function ChangeMode() {
    const radioButtons = document.querySelectorAll('input[name="mode"]');

    let selectedMode = "";

    for (const mode of radioButtons) {
        if (mode.checked) {
            selectedMode = mode.value;
            break;
        }
    }

    SetMode(selectedMode);

    //if (selectedMode === "Lightmode") {
    //    var sheet = document.styleSheets[0];
    //    sheet.insertRule(":root{--BackgroundColor: rgb(255, 255, 255); --LayoutColor: rgb(253, 253, 253); --ShadowColor: rgb(110, 110, 110); --BorderColor: rgb(0, 0, 0); --colortext: black}");
    //}

    //if (selectedMode === "Darkmode") {

    //}

    
}

function SetMode(mode) {
    if (mode === "Lightmode") {
        document.documentElement.style.setProperty('--BackgroundColor', 'rgb(255, 255, 255)');
        document.documentElement.style.setProperty('--LayoutColor', 'rgb(253, 253, 253)');
        document.documentElement.style.setProperty('--ShadowColor', 'rgb(110, 110, 110)');
        document.documentElement.style.setProperty('--BorderColor', 'rgb(0, 0, 0)');
        document.documentElement.style.setProperty('--colortext', 'black');
        localStorage.setItem("Mode", "Lightmode");
    }

    if (mode === "Darkmode") {
        document.documentElement.style.setProperty('--BackgroundColor', 'rgb(18, 18, 18)');
        document.documentElement.style.setProperty('--LayoutColor', 'rgb(15, 15, 15)');
        document.documentElement.style.setProperty('--ShadowColor', 'rgb(27, 27, 27)');
        document.documentElement.style.setProperty('--BorderColor', 'rgb(255, 255, 255)');
        document.documentElement.style.setProperty('--colortext', 'white');
        localStorage.setItem("Mode", "Darkmode");
    }
}