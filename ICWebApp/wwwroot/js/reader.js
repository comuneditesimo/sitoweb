const synth = window.speechSynthesis;
function startReading() {
    container = document.querySelector("#topelement");

    inputTxt = "";
    
    inputs = container.querySelectorAll("p:not(.no-read), h1:not(.no-read), h2:not(.no-read), h3:not(.no-read), a:not(.no-read)");
    inputs.forEach((item) => {
        if (hasSomeParentTheClass(item, "no-read") == false)
        {
            inputTxt = inputTxt + " " + item.innerText;
        }
    });

    utterThis = new SpeechSynthesisUtterance(inputTxt);
    utterThis.rate = 0.85;

    utterThis.onpause = (event) => {
        event.utterance.text.charAt(event.charIndex);
    };

    synth.speak(utterThis);
}
function hasSomeParentTheClass(element, classname) {
    if (element != null && element.classList != null && element.classList.contains(classname) == true)
        return true;

    if (element.parentNode == null)
        return false;

    return hasSomeParentTheClass(element.parentNode, classname);
}

function stopReading() {
    synth.cancel();
}