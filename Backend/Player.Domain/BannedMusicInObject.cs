using System;
using Player.Domain.Base;

namespace Player.Domain
{
    public class BannedMusicInObject : Entity
    {
        public ObjectInfo Object { get; set; }
        public Guid ObjectId { get; set; }
        public MusicTrack MusicTrack { get; set; }
        public Guid MusicTrackId { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
    }
}