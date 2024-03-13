using System;

namespace Player.Services.Configurations
{
    public class PlaylistSpaceManagerConfiguration
    {
        public int MaxAdvertSequence { get; set; }

        public TimeSpan MinSameTrackGap { get; set; }
        
        public int DelayBetweenTracksInSeconds { get; set; }
    }
}