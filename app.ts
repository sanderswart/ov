/// <reference path="../node_modules/definitely-typed-jquery/jquery.d.ts" />
/// <reference path="ovcs.ts" />
/// <reference path="geo.ts" />

/*
*   OV App
*
*   Functionality between device/browser and other classes.
*
*/

function main() {
    //$("head").load("head.html")
    //$("mainDIV").load("menu.html")
    //ovcs.log("Done loading", "$(document).ready", App.className);
    console.log("Start - app.js - main");
    ovcs.initialize();
    ovrs.initialize();
    App.initialize();
    Map.initialize();
    //$("#app").show();
    //$("#map").hide();
    console.log("End - app.js - main");
}

class App {
    static className: string;
    static ovrs: ovrs;
    static ovcs: ovcs;

    // Application Constructor
    static initialize() {
        this.className = "App";

        console.log("Start - App.js - initialize");

        this.ovrs = new ovrs();
        this.ovcs = new ovcs();
        ovcs.log("Start", "constructor", this.className);

        // this.bindEvents();
        //this.registerClicks();
        this.receivedEvent('deviceready');

        console.log("End - App.js - initialize");
    }

    // Bind Event Listeners
    static bindEvents() {
        document.addEventListener('deviceready', this.onDeviceReady, false);
        $(window).bind('beforeunload', ovcs.clearWatch);
    }

    // deviceready Event Handler
    static onDeviceReady() {
        ovcs.log("Start", "onDeviceReady", this.className);
        this.registerClicks();
        this.receivedEvent("deviceready");
        ovcs.log("End", "onDeviceReady", this.className);
    }

    // Register button click events
    static registerClicks() {
        ovcs.log("Executing", "registerClicks", App.className);
        // Unregister clicks
        $('#btnHome').off('click');
        $('#btnToggleView').off('click');
        $('#btnLocation').off('click');
        $('#btnGetDrone').off('click');
        $('#btnGetInventoryPilot').off('click');
        $('#btnGetInventoryDrone').off('click');
        // Register clicks
        $("#btnHome").click(this.goHome);
        $("#btnToggleView").click(this.toggleView);
        $("#btnLocation").click(this.getLocation);
        $("#btnGetDrone").click(this.getDrone);
        $("#btnGetInventoryPilot").click(this.getInventoryPilot);
        $("#btnGetInventoryDrone").click(this.getInventoryDrone);
    }

    // Update DOM on a Received Event
    static receivedEvent(sId: string) {
        //$("p.event.received").show();
        //$("p.event.listening").hide();
        ovcs.log("Event: " + sId, "receivedEvent", this.className);
    }

    // Update element with file content
    static setElementFileContent(sElement: string, sFile: string) {
        let xhr = new XMLHttpRequest();
        xhr.open("GET", sFile, true);
        xhr.onreadystatechange = function () {
            if (this.readyState !== 4) return;
            if (this.status !== 200) return;
            document.getElementById(sElement).innerHTML = this.responseText;
            App.registerClicks();
        };
        xhr.send();
    }

    // Update element with string
    static setElementStringContent(sElement: string, sString: string) {
        document.getElementById(sElement).innerHTML = sString;
        App.registerClicks();
    }

    // Navigate to page
    static setPage(pageName: any) {
        //window.location = pageName;
    }

    // Get page from URL
    static getPage() {
        window.location.pathname.split("/").pop();
    }

    // Open/close menu
    static menuToggle() {
        if ($("#navBtn").hasClass("navOpen")) {
            $("#navBtn").removeClass("navOpen");
            $("#listMenu").removeClass("listOpen");
            $("#shadowbox").removeClass("visible");
        } else {
            $("#navBtn").addClass("navOpen");
            $("#listMenu").addClass("listOpen");
            $("#shadowbox").addClass("visible");
        }
    }

    // Navigate to homepage
    static goHome() {
        // app.showDIV("app");
        this.setPage("index.html");
    }

    // Switch to Pilot / Drone view
    static toggleView() {
        $("#btnToggleView").text(ovcs.toggleView() ? "DRONE" : "PILOT");
    }

    // Start watching position data
    static getLocation() {
        ovcs.log("Plugin GET LOCATION CALLED", "getLocation", this.className);
        let btnText = "FINDING...";
        ovcs.log("btnLocation.text: " + $("#btnLocation").text(), "getLocation", this.className);
        if ($("#btnLocation").text() === btnText) {
            ovcs.log("STOP", "getLocation", this.className);
            App.showLocationText("GET LOCATION");
            ovcs.stopPolling();
            ovcs.clearWatch();
        } else {
            ovcs.log("START", "getLocation", this.className);
            App.showLocationText(btnText);
            // TEST
            // Start with a call to the Web API for components in range of Pilot / Drone
            ovcs.startPolling();            // Timeout = 2000
            ovcs.watchPosition();
            // Draw map
            //Map.showMap(52.08, 6.14);
        }
    }

    // Send Drone to Pilot
    static getDrone() {
        ovcs.drone.amHoming(true);
    }

    // Get Pilots inventory list from web API
    static getInventoryPilot() {
        this.getInventory(ovcs.pilot.ID);
    }

    // Get Drones inventory list from web API
    static getInventoryDrone() {
        this.getInventory(ovcs.drone.ID);
    }

    // Get inventory list for Components ID from web API
    static getInventory(lCompID: number) {
        ovcs.log("Get Inventory clicked for " + lCompID, "getInventory", this.className);
        ovrs.getData("inventory", lCompID.toString());
    }

    // Navigate to inventory page
    static showInventory(compsReceived: Array<Component>, lCompID: number) {
        ovcs.log("Executing for Component " + lCompID, "showInventory", this.className);
        this.setPage("inventory.html");
        let titleDIV = "U N K N O W N";
        if (lCompID === ovcs.pilot.ID) {
            titleDIV = "P I L O T";
        } else if (lCompID === ovcs.drone.ID) {
            titleDIV = "D R O N E";
        }
        // Add list to DIV
        ovcs.log("Setting DIV title to '" + titleDIV + "', then adding " + compsReceived.length + " components", "showInventory", this.className);
        $("#invCanvas").empty();
        $("#invCanvas").append('<br /><br /><br /><br />');
        $("#invCanvas").append('<h1 class="inventory"> I N V E N T O R Y <-> ' + titleDIV + ' </h1>');
        $("#invCanvas").append('<div id="alistapartbutton"></div>');
        $("#invCanvas div").append('<ul></ul>');
        for (let i = 0; i < compsReceived.length; i++) {
            let invComp = compsReceived[i];
            $("#invCanvas div ul").append('<li><a href="#">ID: ' + invComp.ID + ' / Type: ' + invComp.Type + ' / Created: ' + invComp.Created + '</a></li>');
            if (i > 6 && compsReceived.length > 10) {
                $("#invCanvas div ul").append('<li><a href="#">... (and ' + (compsReceived.length - i) + ' more)</a></li>');
                break;
            }
        }
    }

    // Display message on btnLocation
    static showLocationText(buttonText?: string) {
        if (buttonText) {
            if ($("#btnLocation").text() === buttonText) {
                $("#btnLocation").text("GET LOCATION");
            } else {
                $("#btnLocation").text(buttonText);
            }
        }
    }

    // Display toastr
    static showToast(sMessage: string) {
        toastr.info(sMessage)
    }

    // Log message from method in class
    static log(sMessage: string, sMethod?: string, sClass?: string) {
        let logMessage = new Date().toLocaleString();
        if (sClass) {
            logMessage = logMessage + '::[' + sClass + ']';
        }
        if (sMethod) {
            logMessage = logMessage + '::(' + sMethod + ')';
        }
        console.log(logMessage + '->' + sMessage);
    }
}