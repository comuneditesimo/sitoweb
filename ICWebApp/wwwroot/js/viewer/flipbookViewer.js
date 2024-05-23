function OpenFlipbookFromPDF(viewerContainerClass, pdfURL) {
    $('.' + viewerContainerClass).FlipBook({ 'pdf': pdfURL });
}
function OpenFlipbookFromImage(viewerContainerClass, imageURL) {
    $('.' + viewerContainerClass).FlipBook({
        pageCallback: function (n) {
            return {
                type: 'image',
                src: imageURL,
                interactive: false
            };
        }
    });
}
function OpenFlipbookFromHtml(viewerContainerClass, htmlURL) {
    $('.' + viewerContainerClass).FlipBook({
        pageCallback: function (n) {
            return {
                type: 'html',
                src: htmlURL,
                interactive: true
            };
        }
    });
}