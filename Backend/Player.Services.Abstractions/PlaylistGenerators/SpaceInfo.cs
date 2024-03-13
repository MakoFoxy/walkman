using System;

namespace Player.Services.Abstractions.PlaylistGenerators
{
    public class SpaceInfo
    {
        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }
        public SpaceType Type { get; set; }
    }
}