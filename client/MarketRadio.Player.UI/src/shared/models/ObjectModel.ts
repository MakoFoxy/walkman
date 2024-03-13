import SimpleModel from './SimpleModel';
import Settings from './Settings';

export default class ObjectModel {
    public id!: string

    public beginTime!: string

    public endTime!: string

    public attendance!: number

    public actualAddress!: string

    public legalAddress!: string

    public activityType!: SimpleModel

  public city!: SimpleModel

    public name!: string

    public geolocation!: string

    public serviceCompany!: SimpleModel

    public area!: number

    public rentersCount!: number

    public bin!: string

    public settings?: Settings | null

    public getEndTimeCron(): string {
      const timeSplitted = this.endTime.split(':');
      return `${timeSplitted[1]} ${timeSplitted[0]} * * *`;
    }

    constructor(...initData: Partial<ObjectModel>[]) {
      Object.assign(this, ...initData);
    }
}
