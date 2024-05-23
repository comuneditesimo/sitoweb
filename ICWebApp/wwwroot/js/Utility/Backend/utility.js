function utility_HideContainer(elementid) {
    $("#" + elementid).hide(20);
} function utility_modalHide(elementid) {
    $("#" + elementid).modal('hide');
}
function utility_ToggleContainer(elementid) {
    $("#" + elementid).toggle(20);
}

var reading = false;

function utility_printPage() {
    window.print();
}

var nowPlaying = null;
function utility_readPage() {
    if (reading == false) {
        stopReading();
        startReading();
        reading = true;

        if ($("#stop-listening-node") != null) {
            $("#stop-listening-node").show(0);
            $("#listen-node").hide(0);
        }
    }
    else {
        stopReading();
        reading = false;
        if ($("#stop-listening-node") != null) {
            $("#stop-listening-node").hide(0);
            $("#listen-node").show(0);
        }
    }
}

function utility_sendPage(pagetitle) {
    window.location.href = 'mailto:?subject=' + pagetitle + ' information&body=' + window.location;
}