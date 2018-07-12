/*
 * Pilot position is set with every GPS update
*/

/// <reference path="component.ts" />

class Pilot extends Component {
    constructor(public id: number) {
        super(id, Type.pilot);

        this.stroke = 'white';
        this.fill = 'black';
    }
}