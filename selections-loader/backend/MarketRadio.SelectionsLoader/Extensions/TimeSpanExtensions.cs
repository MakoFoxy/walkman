using System;

namespace MarketRadio.SelectionsLoader.Extensions
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan Round(this TimeSpan time)
        {
            return new TimeSpan(time.Hours, time.Minutes, time.Seconds);
        }
    }
}