using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Player.Domain;
using Player.DTOs;

namespace Player.Services.Abstractions
{
    public interface IPlaylistGenerator
    {
        Task<PlaylistGeneratorResult> Generate(ObjectInfo objectInfo, DateTime date, CancellationToken cancellationToken = default);
        Task<PlaylistGeneratorResult> Generate(Guid objectId, DateTime date, CancellationToken cancellationToken = default);
    }

    public class PlaylistGeneratorResult
    {
        public PlaylistGeneratorStatus Status { get; set; }
        public Playlist Playlist { get; set; }
        public ICollection<Advert> NotFittedAdverts { get; set; } = new List<Advert>();
        public ICollection<string> DebugInfo { get; set; } = new List<string>();
    }

    public enum PlaylistGeneratorStatus
    {
        None,
        Generated,
        NotGenerated,
        Delete,
    }
}
