using System;
using System.Collections.Generic;
using Player.Domain.Base;

namespace Player.Domain
{
    /// <summary>
    /// Подборка музыкальных треков
    /// </summary>
    public class Selection : Entity
    {
        public string Name { get; set; }
        public DateTimeOffset DateBegin { get; set; }
        public DateTimeOffset? DateEnd { get; set; }
        public bool IsPublic { get; set; }
        public Guid? OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public ICollection<MusicTrackSelection> MusicTracks { get; set; } = new List<MusicTrackSelection>();
        public ICollection<ObjectSelection> Objects { get; set; } = new List<ObjectSelection>();
    }
}
