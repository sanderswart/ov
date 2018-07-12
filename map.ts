/// <reference path="../node_modules/definitely-typed-jquery/jquery.d.ts" />
/// <reference path="../scripts/typings/googlemaps/google.maps.d.ts" />

/*
*   Map - Leaflet
*
*   Maintain map (centered on Pilot or Drone)
*
*/

class Map {
    static className: string;
    static iLat: number;
    static iLon: number;
    static geoCoder: google.maps.Geocoder;
    static map: google.maps.Map;

    // Initialize new instance of Map class
    static initialize() {
        this.className = "Map";
        ovcs.log("Start", "initialize", ovcs.className);

        this.geoCoder = new google.maps.Geocoder();
    }

    static showMap() {
        ovcs.log("Calling Map.showMap", "gpsSuccess", ovcs.className);
        let mapOptions = {
            center: new google.maps.LatLng(this.iLat, this.iLon),
            zoom: 8,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };
        this.map = new google.maps.Map(document.getElementById("appContent"), mapOptions);
    }
}