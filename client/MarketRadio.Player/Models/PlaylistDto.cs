using System;
using System.Collections.Generic;
using Player.ClientIntegration;

namespace MarketRadio.Player.Models
{
    public class PlaylistDto
    {
        public Guid Id { get; set; }
        public ICollection<TrackDto> Tracks { get; set; } = new List<TrackDto>();
        public bool Overloaded { get; set; }
        public DateTime Date { get; set; }
    }
}