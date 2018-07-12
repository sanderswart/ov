/*
 * Component is the base for Types Pilot, Drone, etc.
 * and represents the component model in the ov database
 * received via the Web API ovrs
*/

enum Type {
    component = 1,
    pilot = 2,
    drone = 3,
    part = 4,
    food = 5,
    accessory = 6,
    tool = 7
}

class Component {
    className: string;
    ID: number;
    Type: Type;
    Lon: number;
    Lat: number;
    Parent: number;
    Visible: Date;
    Created: Date;
    Modified: Date;
    Deleted: Date;

    constructor(public id: number, public type?: Type) {
        this.className = "Component";
        this.ID = id;
        this.Type = type ? type : Type.component;
    }

    print() {
        return 'ID: ' + this.ID + ', type: ' + this.Type;
    }

}