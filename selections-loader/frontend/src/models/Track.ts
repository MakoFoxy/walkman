import {SimpleModel} from '@/models/SimpleModel';

export class Track extends SimpleModel {
    public length = '';

    public uploaded = false

    public uploadInProgress = false

    public genres = new Array<SimpleModel>();

    constructor(init?: Partial<Track>) {
        super();
        Object.assign(this, init);
    }
}
