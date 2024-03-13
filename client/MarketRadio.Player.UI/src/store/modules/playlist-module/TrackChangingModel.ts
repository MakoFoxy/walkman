export default class TrackChangingModel {
    private _startTime: Date

    public get startTime(): Date {
      return this._startTime;
    }

    private _isAdvert: boolean

    public get isAdvert(): boolean {
      return this._isAdvert;
    }

    constructor(startTime: Date, isAdvert: boolean) {
      this._startTime = startTime;
      this._isAdvert = isAdvert;
    }
}
