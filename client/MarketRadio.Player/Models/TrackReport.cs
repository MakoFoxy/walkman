using System;

namespace MarketRadio.Player.Models
{
    public class TrackReport
    {
        public Guid ObjectId { get; set; }
        public Guid AdvertId { get; set; }
        public DateTime  Start { get; set; }
        public DateTime End { get; set; }
    }
}