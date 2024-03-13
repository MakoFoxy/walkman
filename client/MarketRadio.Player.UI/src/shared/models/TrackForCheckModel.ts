export default class TrackForCheckModel {
    private _trackType!: string

    public get trackType(): string {
      return this._trackType;
    }

    public set trackType(v: string) {
      this._trackType = v;
    }

    private _trackId!: string

    public get trackId(): string {
      return this._trackId;
    }

    public set trackId(v: string) {
      this._trackId = v;
    }

    private _hash!: string

    public get hash(): string {
      return this._hash;
    }

    public set hash(v: string) {
      this._hash = v;
    }
}
