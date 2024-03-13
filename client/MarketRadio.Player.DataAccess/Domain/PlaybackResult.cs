using System;
using MarketRadio.Player.DataAccess.Domain.Base;

namespace MarketRadio.Player.DataAccess.Domain
{
    public class PlaybackResult : Entity
    {
        public Playlist Playlist { get; set; } = null!;
        public Guid PlaylistId { get; set; }
        public Track Track { get; set; } = null!;
        public Guid TrackId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public PlaybackStatus Status { get; set; }
        public string? AdditionalInfo { get; set; }
    }
}