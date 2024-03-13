using System;
using System.ComponentModel.DataAnnotations.Schema;
using MarketRadio.Player.DataAccess.Domain.Base;

namespace MarketRadio.Player.DataAccess.Domain
{
    [Table("PlaylistTracks")]
    public class PlaylistTrack : Entity
    {
        public Playlist Playlist { get; set; } = null!;
        public Guid PlaylistId { get; set; }
        public Track Track { get; set; } = null!;
        public Guid TrackId { get; set; }
        
        public DateTime PlayingDateTime { get; set; }
    }
}