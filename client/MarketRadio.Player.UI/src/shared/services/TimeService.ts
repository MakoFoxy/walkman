class TimeService {
  public compareTime(d1: Date, d2: Date): boolean {
    return d1.getHours() >= d2.getHours()
                && d1.getMinutes() >= d2.getMinutes()
                && d1.getSeconds() > d2.getSeconds();
  }
}

export default new TimeService();
