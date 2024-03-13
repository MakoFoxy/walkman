import Track from './Track';

export default class Playlist {
    public id = ''

    public tracks: Track[] = new Array<Track>()

    public overloaded = false

    public date!: Date
}
