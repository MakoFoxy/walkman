using System;
using Player.Domain.Base;

namespace Player.Domain
{
    public class MusicTrackSelection : Entity
    {
        public MusicTrack MusicTrack { get; set; }
        public Guid MusicTrackId { get; set; }
        public Selection Selection { get; set; }
        public Guid SelectionId { get; set; }
        public int Index { get; set; }
    }
}
