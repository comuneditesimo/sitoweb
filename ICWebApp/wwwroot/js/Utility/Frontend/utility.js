function utility_modalHide(elementid) {
    console.log(elementid);
    $("#" + elementid).modal('hide');
}

function injectMetaTag(content) {
    $("#metaelement").html(content);
}

function openDocument(url, target) {
    window.open(url, target);
}
function openInNewTab(url) {
    window.open(url, "_blank");
}