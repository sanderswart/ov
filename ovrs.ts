/*
*   OV Restfull-service
*
*   Web API support
*
*/

class ovrs {
    static className: string;
    static webAPIURL: string;
    static pilotID: number;
    static droneID: number;

    // OV Restfull-service constructor
    static initialize() {
        this.className = "ovrs";
        this.webAPIURL = "http://37.97.242.146:8080/api/";
        this.getPilotID();
        this.getDroneID();
    }

    static getPilotID() {
        if (this.pilotID <= 0) {
            // Use Web API with some local/session/profile params to retrieve Pilot ID
            // TODO
            this.pilotID = 2;
        }
        return this.pilotID;
    }

    static getDroneID() {
        if (this.droneID <= 0) {
            // Use Web API with some local/session/profile params to retrieve Drone ID
            // TODO
            this.droneID = 3;
        }
        return this.droneID;
    }

    static getDataComponents() {
        ovcs.log("Executing getDataComponents()", "getDataComponents", this.className);
        this.getData("component");
    }

    static getDataComponent(id: number, callback: (data: Array<Component>) => any) {
        ovcs.log("Executing getDataComponent(" + id + ")", "getDataComponent(id)", this.className);
        this.getData("component", id.toString(), callback);
    }

    static getDataTypes() {
        ovcs.log("Executing getDataTypes()", "getDataTypes", this.className);
        this.getData("type");
    }

    static getDataProps() {
        ovcs.log("Executing getDataProps()", "getDataProps", this.className);
        this.getData("prop");
    }

    static getData(sObjectName: string, param?: string, callback?: (data: Array<Component>) => any) {
        let sURL = this.webAPIURL + sObjectName + "/" + (param ? param : "");
        let sCallback = callback ? "callback: " + callback : "no callback";
        ovcs.log("Executing getData(" + sObjectName + ") with URL: " + sURL + ", " + sCallback, "getData", this.className);
        if (typeof param === 'undefined') { param = ''; }
        ovcs.log("Executing short get", "getData", this.className);
        $.getJSON(sURL, callback);
        // console.log('Executing full get');
        // $.ajax({
        // url: sURL,
        // dataType: 'json',
        // success: function(result){
        // console.log("SUCCESS - token recieved: " + result.token);
        // },
        // error: function(request, textStatus, errorThrown) {
        // console.log('ERROR - textStatus: ' + textStatus);
        // },
        // complete: function(request, textStatus) { //for additional info
        // console.log('COMPLETE - responseText: ' + request.responseText);
        // console.log('COMPLETE - textStatus: ' + textStatus);
        // }
        // });
        ovcs.log("Executed getData", "getData", this.className);
    }

    static putComponent(component: Component) {
        ovcs.log("Updating position for component " + component.ID + " to Lon=" + component.Lon + " / Lat=" + component.Lat, "putComponent", this.className);
        $.ajax({
            url: this.webAPIURL + 'component/',
            type: 'PUT',
            dataType: 'json',
            data: component
        }).then(function (data) {
            ovcs.log("Component " + component.ID + " updated", "putComponent", this.className);
        });
    }
}