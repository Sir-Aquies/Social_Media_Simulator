//If its being use should return "false" or "true".
const UseCostum = localStorage.getItem('UseCostum');

//If constum mode is being use, load the colors from localStorage and put then in an array.
if (UseCostum === "true") {
    const SavedCostum = [];
    SavedCostum[0] = localStorage.getItem('ColorBackground');
    SavedCostum[1] = localStorage.getItem('ColorLayout');   
    SavedCostum[2] = localStorage.getItem('ColorText');
    SavedCostum[3] = localStorage.getItem('ColorBorder');
    SavedCostum[4] = localStorage.getItem('ColorShadow');
    SetAppearance(SavedCostum[0], SavedCostum[1], SavedCostum[2], SavedCostum[3], SavedCostum[4]);
}

//Get the current mode (should return "Darkmode" or "Lightmode").
const savedMode = localStorage.getItem('Mode');

//If savedMode in not undefined and UseCostum is "false" or undefined we call SetMode.
//If both conditiones are return false the defaultMode is Darkmode.
if (savedMode && UseCostum === "false") {
    SetMode(savedMode);
}

//This function gets call in Appearance.cshtml by a input radio element.
//The input element passes its value (mode) and it gets send to SetMode.
function ChangeMode(mode) {
    SetMode(mode);
}

//This function gets call in Appearance.cshtml by a button element.
//It lets the user costumize the colors of the web app (not a good idea, might remove it in the future).
//It gets the the color value of input color elements.
function CostumAppareance() {
    const ColorBackground = document.getElementById("ColorBackground").value;
    const ColorLayout = document.getElementById("ColorLayout").value;
    const ColorText = document.getElementById("ColorText").value;
    const ColorBorder = document.getElementById("ColorBorder").value;
    const ColorShadow = document.getElementById("ColorShadow").value;

    SetAppearance(ColorBackground, ColorLayout, ColorText, ColorBorder, ColorShadow);
}

//SetAppearance is use the change the value of CSS variables in WebVariables.css, 
//these variables are the colors of every color declaration in other CSS files (very important).
//It also saves the values in localStorage.
function SetAppearance(ColorBackground, ColorLayout, ColorText, ColorBorder, ColorShadow) {
    document.documentElement.style.setProperty('--bgColor', ColorBackground);
    document.documentElement.style.setProperty('--layoutColor', ColorLayout);
    document.documentElement.style.setProperty('--colorText', ColorText);
    document.documentElement.style.setProperty('--borderColor', ColorBorder);
    document.documentElement.style.setProperty('--ShadowColor', ColorShadow);

    localStorage.setItem("ColorBackground", ColorBackground);
    localStorage.setItem("ColorLayout", ColorLayout);
    localStorage.setItem("ColorText", ColorText);
    localStorage.setItem("ColorBorder", ColorBorder);
    localStorage.setItem("ColorShadow", ColorShadow);
    localStorage.setItem("UseCostum", true);
}

//This function changes the data-theme attibute of the document, which triggers a CSS rule in WebVariables.css
//The rule is only apply to when data-theme is equal the "light".
//When data-theme is set to dark it doesn't do anything, darkmode is the default color.
function SetMode(mode) {
    if (mode === "Lightmode") {
        document.documentElement.setAttribute("data-theme", "light");
        localStorage.setItem("Mode", "Lightmode");
        localStorage.setItem("UseCostum", false);
    }
    else
    {
        document.documentElement.setAttribute("data-theme", "dark");
        localStorage.setItem("Mode", "Darkmode");
        localStorage.setItem("UseCostum", false);
    }
}