using System;
using System.Net.Http;
using System.Threading.Tasks;
using MarketRadio.Player.Models;
using Refit;

namespace MarketRadio.Player.Services.Http
{
    public interface ITrackApi
    {
        [Get("/api/v1/track")]
        Task<HttpContent> DownloadTrack([Query]Guid trackId, [Query]string trackType);

        [Get("/api/v1/track/check")]
        Task<TrackIsCorrectWrapper> CheckTrack([Query]Guid trackId, [Query]string trackType, [Query]string hash);
    }
}