using System;
using System.Collections.Generic;
using Player.Domain.Base;

namespace Player.Domain
{
    public class Playlist : Entity
    {
        public ObjectInfo Object { get; set; }
        public DateTime PlayingDate { get; set; }
        
        [Obsolete("Field not used and will be removed in next release")]
        public bool IsTemplate { get; set; }
        public DateTime CreateDate { get; set; }
        [Obsolete("Field not used and will be removed in next release")]
        public bool RebuildIsNeeded { get; set; }
        
        public double Loading { get; set; }
        public int UniqueAdvertsCount { get; set; }
        public int AdvertsCount { get; set; }
        public bool Overloaded { get; set; }
        
        /// <summary>
        /// Треки которые присутсвуют в плейлисте
        /// </summary>
        public ICollection<MusicTrackPlaylist> MusicTracks { get; set; } = new List<MusicTrackPlaylist>();

        /// <summary>
        /// Реклама, присутствующая в плейлисте
        /// </summary>
        //TODO Переименовать
        public ICollection<AdvertPlaylist> Aderts { get; set; } = new List<AdvertPlaylist>();

        public ICollection<PlaylistInfo> PlaylistInfos { get; set; } = new List<PlaylistInfo>();
    }

    public class PlaylistInfo : Entity
    {
        public string Info { get; set; }
        public DateTime CreateDate { get; set; }
        public Playlist Playlist { get; set; }
        public Guid PlaylistId { get; set; }
    }
}