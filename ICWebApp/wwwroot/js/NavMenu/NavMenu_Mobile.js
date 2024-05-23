function NavMenu_Mobile_OnShow(targetDiv) {
    var element = document.getElementById(targetDiv);
    if (element != null) {
        element.classList.remove("mobile-menu-popup-container-hide");
        element.classList.add("mobile-menu-popup-container-show");
    }
}
function NavMenu_Mobile_OnHide(targetDiv) {
    var element = document.getElementById(targetDiv);

    if (element != null) {
        element.classList.remove("mobile-menu-popup-container-show");
        element.classList.add("mobile-menu-popup-container-hide");
    }
}