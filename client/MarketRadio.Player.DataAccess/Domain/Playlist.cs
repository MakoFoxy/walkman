using System;
using System.Collections.Generic;
using MarketRadio.Player.DataAccess.Domain.Base;

namespace MarketRadio.Player.DataAccess.Domain
{
    public class Playlist : Entity
    {
        public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
        
        public bool Overloaded { get; set; }
        public DateTime Date { get; set; }
    }
}