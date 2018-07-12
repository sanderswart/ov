/*
 * Drone will always move towards its target(Position)
 * and then circle around it with radius
 * Credits to: Latitude/longitude spherical geodesy tools (c) Chris Veness 2002-2016 MIT Licence
 * http://www.movable-type.co.uk/scripts/latlong.html
*/

/// <reference path="geo.ts" />

class Drone extends Component {
    radius: number;
    speed: number;
    currentCoordData: Array<number>;
    targetCoord: Array<number>;
    targetPoint: Array<number>;
    targetReached: boolean;
    homeBound: boolean;
    _targetPosition: ol.Coordinate; // OpenLayers v3 ol.Coordinate in DataProjection 'EPSG:4326' i.e.: [0] = 6.1569906(longitude), [1] = 52.2905538(latitude)
    lastTime: number;
    savedTime: number;
    reached: boolean;
    droneCompsRange: Array<Component>;
    pilotCompsLocalSort: Array<Component>;
    droneCompsLocal: Array<Component>;
    arrLocalComp: Array<Component>;

    constructor(public id: number) {
        super(id, Type.drone);

        this.stroke = 'red';
        this.fill = 'orange';

        this.radius = 10;
        this.speed = 10.8;
        this.targetReached = false;
        this.homeBound = true;
        this.targetPosition = null;
        this.savedTime = Date.now();
        this.reached = false;
    }

    elapsedTime() {
        // Time elapsed in seconds since lastTime
        return (Date.now() - this.lastTime) / 1000;
    }

    distanceTraveled() {
        // Meters
        return this.speed * this.elapsedTime();
    }

    amHoming(newValue) {
        if (newValue) {
            this.homeBound = true;
            this.targetCoord = newValue;
        } else {
            this.homeBound = false;
        }
        return this.homeBound;
    }

    targetPosition(newTargetPosition) {
        // argument: targetPosition - OpenLayers v3 ol.Coordinate in DataProjection 'EPSG:4326' i.e.: [0] = 6.1569906 (longitude), [1] = 52.2905538 (latitude)
        if (newTargetPosition) {
            this._targetPosition = newTargetPosition;
            this.targetReached = false;
        }
        return Geo.featurePosition(this._targetPosition);
    }

    bearingToTarget() {
        // Degrees
        var lat1 = this.targetPoint[0];
        var lon1 = this.targetPoint[1];
        var lat2 = this.currentCoordData[0];
        var lon2 = this.currentCoordData[1];

        var y = Math.sin(lon2 - lon1) * Math.cos(lat2);
        var x = Math.cos(lat1) * Math.sin(lat2) - Math.sin(lat1) * Math.cos(lat2) * Math.cos(lon2 - lon1);

        return (Math.atan2(y, x) * 180 / Math.PI);
    }

    distanceToTarget() {
        // Meters - Haversine formula
        var targetPoint = Geo.dataPosition(Geo.featurePosition(this._targetPosition));
        var lat1 = targetPoint[0];
        var lon1 = targetPoint[1];
        var lat2 = this.featurePosition[0];
        var lon2 = this.featurePosition[1];
        var dLat = Geo.radPerDeg * (lat2 - lat1);
        var dLon = Geo.radPerDeg * (lon2 - lon1);
        var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) + Math.cos(Geo.radPerDeg * lat1) * Math.cos(Geo.radPerDeg * lat2) * Math.sin(dLon / 2) * Math.sin(dLon / 2);
        var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
        return Geo.R * c;
    }

    destinationPoint(curPoint, distance, bearing) {
        // OpenLayers v3 ol.geom.Point
        var lat1 = curPoint[0];
        var lon1 = curPoint[1];
        // 2 radians
        lat1 = lat1 * Geo.radPerDeg;
        lon1 = lon1 * Geo.radPerDeg;
        bearing = bearing * Geo.radPerDeg;
        var lat2 = Math.asin(Math.sin(lat1) * Math.cos(distance / Geo.R) + Math.cos(lat1) * Math.sin(distance / Geo.R) * Math.cos(bearing));
        var lon2 = lon1 + Math.atan2(Math.sin(bearing) * Math.sin(distance / Geo.R) * Math.cos(lat1), Math.cos(distance / Geo.R) - Math.sin(lat1) * Math.sin(lat2));
        // 2 degrees
        lat2 = lat2 / Geo.radPerDeg;
        lon2 = lon2 / Geo.radPerDeg;
        var dataPosition;
        dataPosition.coords.latitude = lat2;
        dataPosition.coords.longitude = lon2;
        return Geo.featurePosition(dataPosition);
    }

    nextCoord() {
        // OpenLayers v3 ol.geom.Point
        var nextPosition;
        var bearing = this.bearingToTarget();
        var previousBearing = bearing;
        var distanceTraveled = this.distanceTraveled();
        var degrees;
        var distanceToTarget = this.distanceToTarget();

        // If targetPoint was set to a point within the previous radius
        var targetSetWithinRadius = this.radius - distanceToTarget > distanceTraveled;

        if (this.targetReached === true || distanceToTarget < this.radius && !targetSetWithinRadius) {
            // Move to next point on circle around target
            if (this.targetReached === false) {
                this.targetReached = true;
            }
            degrees = 360 * distanceTraveled / (2 * Math.PI * this.radius);
            bearing = bearing + degrees;
            nextPosition = this.destinationPoint(this.targetPoint, this.radius, bearing);
        } else {
            // Move towards nearest point where distanceToTarget == radius
            if (!targetSetWithinRadius) {
                bearing = (bearing + 180) % 360;
            }
            nextPosition = this.destinationPoint(Geo.dataPosition(this.featurePosition), distanceTraveled, bearing);
        }
        this.lastTime = Date.now();

        return nextPosition;
    }

    compsLocal(newComps) {
        if (typeof this.droneCompsRange != 'undefined' && this.droneCompsRange.length > 0) {
            ovcs.log("New Components: " + this.droneCompsRange.length, 'compsLocal', 'drone');

            this.pilotCompsLocalSort = [];
            var iMax = 6; // Maybe more = cooler? Elevate to higher level !!!!!
            var sSortProp = 'Sort'; // Elevate to higher level !!!!!

            if (typeof this.droneCompsLocal != 'undefined' && this.droneCompsLocal.length > 0) {
                ovcs.log("Local Components: " + this.droneCompsLocal.length, 'compsLocal', 'drone');

                // Sync new Components into local array:
                // - Move from top to bottom for removing purposes
                // - Remove Component if Sort >= iMax and continue to previous Component
                // - Increment Sort if still in the loop
                // - Add/update Sort for current Component if it's new or lower than saved value

                // Increment Sort in local array to prepare for new Components
                for (var iLocal = this.droneCompsLocal.length; iLocal-- > 0;) {
                    var localComp = this.droneCompsLocal[iLocal];
                    // Check if object should be removed
                    ovcs.log("localComp.ID: " + localComp.ID + ", localComp.Sort: " + localComp[sSortProp], 'compsLocal', 'drone');
                    if (localComp[sSortProp] >= iMax) {
                        // Remove this instance from local array
                        this.droneCompsLocal.splice(iLocal, 1);
                        ovcs.log("localComp.ID: " + localComp.ID + ", localComp.Sort: " + localComp[sSortProp] + " was removed", 'compsLocal', 'drone');
                        continue;
                    }
                    ovcs.log("Local Components (after Max check): " + this.droneCompsLocal.length, 'compsLocal', 'drone');
                    // Increment Sort
                    localComp[sSortProp]++;
                    ovcs.log("localComp.Sort: " + localComp[sSortProp], 'compsLocal', 'drone');
                    ovcs.log("!(localComp.ID in arrLocalComp): " + (!(localComp.ID in this.arrLocalComp)), 'compsLocal', 'drone');
                    ovcs.log("localComp[sSortProp] < arrLocalComp[localComp.ID]: " + (localComp[sSortProp] < this.arrLocalComp[localComp.ID]), 'compsLocal', 'drone');
                    //  Save if not already added or lower to identify lowest Sort per Component
                    if (!(localComp.ID in this.pilotCompsLocalSort) || localComp[sSortProp] < this.pilotCompsLocalSort[localComp.ID]) {
                        this.pilotCompsLocalSort[localComp.ID] = localComp[sSortProp];
                        ovcs.log("arrLocalComp[" + localComp.ID + "] = " + localComp[sSortProp] + " added / updated", 'compsLocal', 'drone');
                    }
                }
                ovcs.log("Local Components (after sync): " + this.droneCompsLocal.length, 'compsLocal', 'drone');
            }

            // Add new components to updated local array
            for (var iNew = 0; iNew < this.droneCompsRange.length; iNew++) {
                var newComp = this.droneCompsRange[iNew];
                // Add new Sort property and set to 1
                newComp[sSortProp] = 1;
                // Save Sort to identify lowest Sort per Component
                this.pilotCompsLocalSort[newComp.ID] = newComp[sSortProp];
                // Add new Component to local array
                this.droneCompsLocal.push(newComp);
                ovcs.log("Added new component: " + newComp.ID, 'compsLocal', 'drone');
            }
            ovcs.log("Local Components (after adding new): " + this.droneCompsLocal.length, 'compsLocal', 'drone');

            // Reset array and wait for new Components to sync
            this.droneCompsRange = [];
        }
        return this.droneCompsLocal;
    }
}