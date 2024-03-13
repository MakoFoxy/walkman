using System.Net.Http;
using System.Threading.Tasks;
using Player.ClientIntegration.Client;
using Player.ClientIntegration.System;
using Refit;

namespace MarketRadio.Player.Services.Http
{
    public interface ISystemService
    {
        [Post("/api/v1/client/send-logs")]
        Task<HttpResponseMessage> SendLogsToServer([Body] DownloadLogsResponse response);
        
        [Get("/api/v1/client/time")]
        Task<CurrentTimeDto> GetServerTime();
    }
}