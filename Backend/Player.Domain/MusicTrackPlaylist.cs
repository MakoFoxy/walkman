using System;
using Player.Domain.Base;

namespace Player.Domain
{
    public class MusicTrackPlaylist : Entity
    {
        public MusicTrack MusicTrack { get; set; }
        public Guid MusicTrackId { get; set; }
        public Playlist Playlist { get; set; }
        
        public Guid PlaylistId { get; set; }
        public DateTime PlayingDateTime { get; set; }
    }
}
