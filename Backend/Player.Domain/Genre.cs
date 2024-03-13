using System.Collections.Generic;
using Player.Domain.Base;

namespace Player.Domain
{
    public class Genre:Entity
    {
        public string Name { get; set; }
        public ICollection<MusicTrackGenre> MusicTracks { get; set; } = new List<MusicTrackGenre>();
    }
}
