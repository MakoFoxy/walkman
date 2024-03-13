using System;
using Player.Domain.Base;

namespace Player.Domain
{
    public class MusicTrackGenre : Entity
    {
        public MusicTrack MusicTrack { get; set; }
        public Guid MusicTrackId { get; set; }
        public Genre Genre { get; set; }
        public Guid GenreId { get; set; }
    }
}
