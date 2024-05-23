var GlobdotNetObjectRef = null;
var openedWindow = null;

function AdobeSign_OpenWindow(dotNetObjectRef, SigningUrl, WindowName, resultDivID) {
    GlobdotNetObjectRef = dotNetObjectRef;

    if (!window.addEventListener) {
        window.attachEvent('onmessage', AdobeSign_EventHandler);
    } else {
        window.addEventListener('message', AdobeSign_EventHandler, false);
    }    
}

function AdobeSign_EventHandler(e) {
    if (e.origin.includes("eu1.adobesign.com") || e.origin.includes("eu1.echosign.com") || e.origin.includes("eu2.echosign.com") || e.origin.includes("eu2.adobesign.com"))
    {
        if (JSON.parse(e.data).type == "ESIGN") {

            GlobdotNetObjectRef.invokeMethodAsync('OnEvent', JSON.stringify("result"));
        }
    }
}

function AdobeSign_FireSignedEvent() 
{
    GlobdotNetObjectRef.invokeMethodAsync('OnEvent', JSON.stringify("result"));
}