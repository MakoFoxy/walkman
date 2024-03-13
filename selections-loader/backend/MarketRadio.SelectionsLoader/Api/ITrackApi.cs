using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.SelectionsLoader.Models;
using Refit;

namespace MarketRadio.SelectionsLoader.Api
{
    public interface ITrackApi
    {
        [Get("/api/v1/genres?page=1&itemsPerPage=10000")]
        Task<ICollection<GenreDto>> GetGenres([Header("Authorization")] string token);
        
        [Post("/api/v1/genres")]
        Task<Guid> CreateGenre(SimpleDto genre, [Header("Authorization")] string token);

        [Get("/api/v1/music/all")]
        Task<ICollection<TrackDto>> GetMusicTracks([Header("Authorization")] string token);
        
        [Get("/api/v1/music/all")]
        Task<string> GetMusicTracksRaw([Header("Authorization")] string token);

        [Post("/api/v1/music")]
        [Multipart]
        Task<HttpResponseMessage> SaveMusic(
            string genre,
            Guid musicTrackId,
            [AliasAs("musicFiles")] IEnumerable<StreamPart> files,
            [Header("Authorization")] string token,
            CancellationToken cancellationToken);
    }
}