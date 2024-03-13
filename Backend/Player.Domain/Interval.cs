using System;
using Player.Domain.Base;

namespace Player.Domain
{
    public class Interval : Entity
    {
        public TrackType TrackType { get; set; }
        public TimeSpan BeginTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
