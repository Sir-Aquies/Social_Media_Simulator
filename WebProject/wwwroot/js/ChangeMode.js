const UseCostum = localStorage.getItem('UseCostum');

if (UseCostum === "true") {
    const SavedCostum = [];
    SavedCostum[0] = localStorage.getItem('ColorBackground');
    SavedCostum[1] = localStorage.getItem('ColorLayout');   
    SavedCostum[2] = localStorage.getItem('ColorText');
    SavedCostum[3] = localStorage.getItem('ColorBorder');
    SavedCostum[4] = localStorage.getItem('ColorShadow');
    SetAppearance(SavedCostum[0], SavedCostum[1], SavedCostum[2], SavedCostum[3], SavedCostum[4]);
}

const savedTheme = localStorage.getItem('Mode');
if (savedTheme && UseCostum === "false") {
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

function CostumAppareance() {
    const ColorBackground = document.getElementById("ColorBackground").value;
    const ColorLayout = document.getElementById("ColorLayout").value;
    const ColorText = document.getElementById("ColorText").value;
    const ColorBorder = document.getElementById("ColorBorder").value;
    const ColorShadow = document.getElementById("ColorShadow").value;

    SetAppearance(ColorBackground, ColorLayout, ColorText, ColorBorder, ColorShadow);
}

function SetAppearance(ColorBackground, ColorLayout, ColorText, ColorBorder, ColorShadow) {
    document.documentElement.style.setProperty('--BackgroundColor', ColorBackground);
    document.documentElement.style.setProperty('--LayoutColor', ColorLayout);
    document.documentElement.style.setProperty('--colortext', ColorText);
    document.documentElement.style.setProperty('--BorderColor', ColorBorder);
    document.documentElement.style.setProperty('--ShadowColor', ColorShadow);

    localStorage.setItem("ColorBackground", ColorBackground);
    localStorage.setItem("ColorLayout", ColorLayout);
    localStorage.setItem("ColorText", ColorText);
    localStorage.setItem("ColorBorder", ColorBorder);
    localStorage.setItem("ColorShadow", ColorShadow);
    localStorage.setItem("UseCostum", true);
}

function SetMode(mode) {
    if (mode === "Lightmode") {
        document.documentElement.style.setProperty('--BackgroundColor', 'rgb(255, 255, 255)');
        document.documentElement.style.setProperty('--LayoutColor', 'rgb(250, 250, 250)');
        document.documentElement.style.setProperty('--ShadowColor', 'rgb(110, 110, 110)');
        document.documentElement.style.setProperty('--BorderColor', 'rgb(0, 0, 0)');
        document.documentElement.style.setProperty('--colortext', 'black');
        localStorage.setItem("Mode", "Lightmode");
        localStorage.setItem("UseCostum", false);
    }

    if (mode === "Darkmode") {
        document.documentElement.style.setProperty('--BackgroundColor', 'rgb(18, 18, 18)');
        document.documentElement.style.setProperty('--LayoutColor', 'rgb(15, 15, 15)');
        document.documentElement.style.setProperty('--ShadowColor', 'rgb(27, 27, 27)');
        document.documentElement.style.setProperty('--BorderColor', 'rgb(255, 255, 255)');
        document.documentElement.style.setProperty('--colortext', 'white');
        localStorage.setItem("Mode", "Darkmode");
        localStorage.setItem("UseCostum", false);
    }
}