function ChangeMode() {
    const radioButtons = document.querySelectorAll('input[name="mode"]');

    let selectedMode = "";

    for (const mode of radioButtons) {
        if (mode.checked) {
            selectedMode = mode.value;
            break;
        }
    }

    if (selectedMode === "Lightmode") {
        document.documentElement.style.setProperty('--BackgroundColor', 'white');
        document.documentElement.style.setProperty('--LayoutColor', 'rgb(242, 242, 242)');
        document.documentElement.style.setProperty('--colortext', 'black');
    }

    if (selectedMode === "Darkmode") {
        document.documentElement.style.setProperty('--BackgroundColor', 'rgb(18, 18, 18)');
        document.documentElement.style.setProperty('--LayoutColor', 'rgb(15, 15, 15)');
        document.documentElement.style.setProperty('--colortext', 'white');
    }
}