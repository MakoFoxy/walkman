/* eslint-disable  @typescript-eslint/no-explicit-any */

export default class CommunicationModel {
  public get event(): string {
    return this._event;
  }

  public set event(v: string) {
    this._event = v;
  }

  public get data(): any {
    return this._data;
  }

  public set data(v: any) {
    this._data = v;
  }

  private _event!: string

  private _data: any

  public ConvertDataTo<T>(): T {
    return this._data;
  }

  public fillFromJson(json: string): void {
    const d = JSON.parse(json) as CommunicationModel;
    this._data = d._data;
    this._event = d._event;
  }

  constructor(...initData: Partial<CommunicationModel>[]) {
    Object.assign(this, ...initData);
  }
}
