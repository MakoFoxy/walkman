import {SimpleModel} from '@/models/SimpleModel';

export class TrackInSelection extends SimpleModel {

    public length!: number

    public order!: number

    constructor(init?: Partial<TrackInSelection>) {
        super();
        Object.assign(this, init);
    }
}
