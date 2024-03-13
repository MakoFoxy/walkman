import {SimpleModel} from '@/models/SimpleModel';
import {TrackInSelection} from '@/models/TrackInSelection';

export class Selection extends SimpleModel {

    public isPublic!: boolean

    public created!: boolean

    public dateBegin!: Date

    public dateEnd: Date | null = null

    public tracks = new Array<TrackInSelection>();

    constructor(init?: Partial<Selection>) {
        super();
        Object.assign(this, init);
    }
}
