using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Player.Domain;
using Player.DTOs;

namespace Player.Services.Abstractions
{
    public interface IMusicTrackCreator
    {
        Task<List<MusicTrack>> CreateMusicTracks(MusicTrackCreatorData creatorData);
    }

    public class MusicTrackCreatorData
    {
        public IReadOnlyCollection<SimpleDto> Tracks { get; set; } = new List<SimpleDto>();
        public int MaxIndex { get; set; }
        public string BasePath { get; set; }
        public Guid MusicTrackTypeId { get; set; }
        public Guid GenreId { get; set; }
        public Guid UserId { get; set; }
    }
}