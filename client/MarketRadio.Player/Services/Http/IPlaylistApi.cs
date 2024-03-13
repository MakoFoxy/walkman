using System;
using System.Net.Http;
using System.Threading.Tasks;
using MarketRadio.Player.Models;
using Refit;

namespace MarketRadio.Player.Services.Http
{
    public interface IPlaylistApi
    {
        [Get("/api/v1/playlist")]
        Task<PlaylistWrapper> GetPlaylist([Query] Guid objectId, [Query] DateTime date);

        [Post("/api/v1/report")]
        Task<HttpContent> SendTrackReport([Body] TrackReport trackReport);
    }
}