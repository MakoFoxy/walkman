using System;

namespace MarketRadio.Player.Helpers
{
    public static class DateTimeExtensions
    {
        public static DateTimeOffset ResetToSeconds(this DateTimeOffset dateTime)
        {
            return dateTime.AddMilliseconds(-dateTime.Millisecond);
        }
    }
}