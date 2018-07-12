/*
*   Menu and layout support
*/
var mnu = {
    showDIV: function (sDIV) {
        // Skip if requested DIV is already visible
        if (!$("#" + sDIV).is(":visible")) {
            let supportedDIVs = "app,map,inv".split(",");
            for (var i = 0; i < supportedDIVs.length; i++) {
                let checkDIV = supportedDIVs[i];
                if (checkDIV != sDIV && $("#" + checkDIV).is(":visible")) {
                    // Grab menu from visible DIV and hide
                    var menu = $("#" + checkDIV + "Menu").html();
                    $("#" + checkDIV + "Menu").html("");
                    $("#" + checkDIV).hide();
                    // Append menu to requested DIV and show
                    $("#" + sDIV + "Menu").html(menu);
                    $("#" + sDIV + "Menu").css({ left: "68px" });
                    $("#" + sDIV).show();
                    // Re-init requested DIV
                    if (sDIV == "app") {
                        navigator.geolocation.clearWatch(ovcs.watchId);
                        App.registerClicks();
                    } else if (sDIV == "map") {
                        App.registerClicks();
                        document.getElementById("btnLocation").removeEventListener("click", App.getLocation, false);
                        App.showLocationText();
                    } else if (sDIV == "inv") {
                    }
                    return true;
                }
            }
        }
        return false;
    }
}