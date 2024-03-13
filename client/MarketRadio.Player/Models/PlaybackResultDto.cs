using System;
using MarketRadio.Player.DataAccess.Domain;

namespace MarketRadio.Player.Models
{
    public class PlaybackResultDto
    {
        public Guid PlaylistId { get; set; }
        public Guid TrackId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public PlaybackStatus Status { get; set; }
        public string? AdditionalInfo { get; set; }
    }
}