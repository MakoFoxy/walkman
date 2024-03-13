using System;

namespace MarketRadio.Player.Helpers
{
    public static class DateTimeExtensions
    {
        public static DateTime RoundToSeconds(this DateTime date)
        {
            return date.Millisecond < 500
                ? date.AddMilliseconds(-date.Millisecond)
                : date.AddMilliseconds(1000 - date.Millisecond);
        }
    }
}