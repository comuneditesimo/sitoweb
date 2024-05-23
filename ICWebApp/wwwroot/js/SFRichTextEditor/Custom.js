window.Richtexteditor = {
    selection: null,
    ranges: null,
    rteObj: null,
    saveSelection: null,
    created: function (rteID) {
        window.Richtexteditor.selection = new sf.richtexteditor.NodeSelection();
        window.Richtexteditor.rteObj = document.getElementById(rteID).ej2_instances[0];
        window.Richtexteditor.selection = window.Richtexteditor.rteObj.formatter.editorManager.nodeSelection;
        window.Richtexteditor.rteObj.blur = function () {
            window.Richtexteditor.ranges = window.Richtexteditor.selection.getRange(document);
            window.Richtexteditor.saveSelection = window.Richtexteditor.selection.save(window.Richtexteditor.ranges, document);
        };
    },
    onfocusout: function () {
        if (window.Richtexteditor.saveSelection) {
            window.Richtexteditor.saveSelection.restore();
        }
    }
};