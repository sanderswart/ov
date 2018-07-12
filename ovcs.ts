/*
*   OV Client-Side
*
*   Client-side support
*
*/

/// <reference path="component.ts" />
/// <reference path="pilot.ts" />
/// <reference path="drone.ts" />

class gpsOptions {
    maximumAge: number; // 3000,
    timeout: number; // 30000,
    enableHighAccuracy: boolean; // true    
}

class ovcs {
    static className: string;
    static viewPilotDrone: boolean;
    static milliseconds2Update: number;
    static firstFix: boolean;
    static gpsOptions: gpsOptions;
    static watchId: number;
    static pollId: number;
    static pilot: Pilot;
    static drone: Drone;
    // DEBUG
    static pollMaxCount: number;
    static pollCount: number;

    // Initialize new instance of OV Client-Side class
    static initialize() {
        this.className = "ovcs";
        ovcs.log("Start", "constructor", this.className);

        this.viewPilotDrone = true; // Default view = Pilot (true)
        this.milliseconds2Update = 2000; // This value should be retrieved from config file
        this.firstFix = true;
        // Options to pass to browser navigator
        this.setGPSOptions(3000, 3000, true);

        // DEBUG
        this.pollCount = 0;
        this.pollMaxCount = 3;
        ovcs.log("Set - pollCount: " + this.pollCount + ", pollMaxCount: " + this.pollMaxCount, "constructor", this.className);

        this.pilot = new Pilot(2);
        this.drone = new Drone(3);

        ovcs.log('Pilot created: ' + this.pilot.ID, "constructor", this.className);
        ovcs.log('Drone created: ' + this.drone.ID, "constructor", this.className);
    }

    // Start polling for components in range of Pilot / Drone
    static startPolling() {
        // setInterval does not seem to end on clearInterval, so for testing one round using setTimeout will do
        //ovcs.pollId = setInterval(ovcs.getComponentsInRange, this.milliseconds2Update);
        ovcs.pollId = setTimeout(ovcs.getComponentsInRange, this.milliseconds2Update);
    }

    // Stop polling for components in range of Pilot / Drone
    static stopPolling() {
        clearInterval(ovcs.pollId);
    }

    // Options to pass to browser navigator
    static setGPSOptions(maximumAge: number, timeout: number, enableHighAccuracy: boolean) {
        this.gpsOptions = new gpsOptions();
        this.gpsOptions.maximumAge = maximumAge;
        this.gpsOptions.timeout = timeout;
        this.gpsOptions.enableHighAccuracy = enableHighAccuracy;
    }

    // Are we looking at the Pilot view (true) or the Drone view (false)?
    static viewPilot() {
        return this.viewPilotDrone;
    }

    // Swithc view to Pilot / Drone
    static toggleView() {
        this.viewPilotDrone = !this.viewPilotDrone;
        this.getComponentsInRange();
        return this.viewPilotDrone;
    }

    // Request components on range of Pilot / Drone
    static getComponentsInRange() {
        let lID;
        if (ovcs.viewPilotDrone === true) {
            lID = ovcs.pilot.ID;
        } else {
            lID = ovcs.drone.ID
        }
        ovcs.log("Executing for Component " + lID, "getComponentsInRange", ovcs.className);
        ovrs.getDataComponent(lID, ovcs.setComponentsInRange);
        // DEBUG
        // ovcs.log("ovcs - Check - pollCount: " + pollCount + ", pollMaxCount: " + pollMaxCount);
        ovcs.log("pollId: " + ovcs.pollId, "getComponentsInRange", ovcs.className);

        //clearInterval(ovcs.pollId); // For testing, just clear immediately
        //ovcs.log("Interval was cleared", "getComponentsInRange", ovcs.className);

        //if (ovcs.pollCount >= ovcs.pollMaxCount) {
        //    clearInterval(ovcs.pollId);
        //    ovcs.log("Interval cleared at poll count " + ovcs.pollCount, "getComponentsInRange", ovcs.className);
        //}
        //ovcs.pollCount++;
    }

    // Process requested components in range of Pilot / Drone
    static setComponentsInRange(data: Array<Component>) {
        ovcs.log("Received " + data.length + " components", "setComponentsInRange", ovcs.className);
        if (ovcs.viewPilotDrone === true) {
            ovcs.log("Pilot view", "setComponentsInRange", ovcs.className);
            ovcs.pilot.setComponentsInRange(data);
        } else {
            ovcs.log("Drone view", "setComponentsInRange", ovcs.className);
            ovcs.drone.setComponentsInRange(data);
        }
    }

    // Tell browser navigator to start checking GPS location data
    static watchPosition() {
        ovcs.watchId = navigator.geolocation.watchPosition(ovcs.gpsSuccess, ovcs.gpsError, this.gpsOptions);
    }

    static watchPosition_offline() {
        console.log("watchPosition_offline");
        //setInterval(function () {
        setTimeout(function () {
            //let position: Position;
            //position.coords.latitude = 52.07;
            //position.coords.longitude = 4.27;
            //ovcs.log("Position - Lat: " + position.coords.latitude + ", Lon: " + position.coords.longitude, "setTimeout", ovcs.className)
            //this.gpsSuccess(position);
        }, 5000)
    }

    // Tell browser navigator to stop checking GPS location data
    static clearWatch() {
        navigator.geolocation.clearWatch(ovcs.watchId);
    }

    // What should happen on a GPS update?
    static gpsSuccess(position: Position) {
        //return function onSuccess(position) {
        ovcs.log("position - Lat: " + position.coords.latitude + ", Lon: " + position.coords.longitude, "gpsSuccess", ovcs.className);
        // GPS update is for Pilot and Drone targets Pilot
        ovcs.pilot.currentCoord(position);
        ovcs.drone.targetCoord = Geo.position2Coords(position);

        if (ovcs.firstFix) {
            //App.setPage("map.html");
            ovcs.log("Calling Map.showMap", "gpsSuccess", ovcs.className);
            Map.iLat = position.coords.latitude;
            Map.iLon = position.coords.longitude;
            Map.showMap();
            // Set Drone starting point at 0.001 degrees from Pilot (this turns out to be West...?) - This should be the same position as Pilot eventually !!!!!
            //this.drone.currentCoord(Geo.dataPosition(position, 0.001));
            ovcs.firstFix = false;
        } else {
            //geo.centerMap();
        }
        //};
    }

    // What should happen on a GPS error?
    static gpsError() {
        return function onError(error) {
            toastr.info("code: " + error.code + "\n message: " + error.message + "\n");
        };
    }

    static updateComponentPosition(point, compID) {
        let component = new Component(compID);

        component.ID = compID;          // Also send authentication proof (session?) !!!!!
        component.Type = -1;            // -1 = Do not update
        component.Lon = point[0];
        component.Lat = point[1];
        component.Parent = -1;          // -1 = Do not update
        component.Visible = null;       // null = 'visible'
        component.Created = null;       // Is never updated
        component.Modified = null;      // Always updated to 'NOW()'
        component.Deleted = null;       // null = 'not deleted'

        ovrs.putComponent(component);
    }

    // Log message to console
    static log(sMessage: string, sMethod: string, sClass: string) {
        // Comment out for production
        console.log(new Date(new Date().getTime()).toISOString().slice(11, -1) + " - " + sClass + "::" + sMethod + "::" + sMessage);
    }

}