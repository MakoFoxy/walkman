using System;

namespace Player.Helpers.Extensions
{
    public static class TimeSpanExtensions
    {
        public static int TrackLengthInSeconds(this TimeSpan length)
        {
            return Convert.ToInt32(length.RoundUp().TotalSeconds);
        }

        public static TimeSpan RoundUp(this TimeSpan length)
        {
            var roundedSeconds = Math.Round(length.TotalSeconds, MidpointRounding.ToPositiveInfinity);
            return TimeSpan.FromSeconds(roundedSeconds);
        }
    }
}