using System;
using System.Collections.Generic;

namespace Player.DTOs
{
    public class PlaylistModel
    {
        public DateTime PlayDate { get; set; }
        public List<TrackModel> Tracks { get; set; } = new List<TrackModel>();
        public ObjectModel Object { get; set; }
    }
}
