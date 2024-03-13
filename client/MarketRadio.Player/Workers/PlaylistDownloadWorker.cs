using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.Models;
using MarketRadio.Player.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Workers
{
    public class PlaylistDownloadWorker : PlayerBackgroundServiceBase
    {
        private const int MaxDownloadPlaylistDays = 1;

        private readonly IServiceProvider _provider;
        private readonly ILogger<PlaylistDownloadWorker> _logger;
        private readonly PlayerStateManager _stateManager;
        private readonly IWebHostEnvironment _env;

        public PlaylistDownloadWorker(IServiceProvider provider, 
            ILogger<PlaylistDownloadWorker> logger,
            PlayerStateManager stateManager,
            IWebHostEnvironment env) : base(stateManager)
        {
            _provider = provider;
            _logger = logger;
            _stateManager = stateManager;
            _env = env;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.WhenAll(WaitForObject(stoppingToken), WaitForPlaylist(stoppingToken));
                
                try
                {
                    if (!_stateManager.PlaylistIsDownloading)
                    {
                        // TODO перенести в конфигурацию
                        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

                        if (!_stateManager.PlaylistIsDownloading)
                        {
                            await DoWork(stoppingToken);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            var from = DateTime.Today.AddDays(1);
            var to = from.AddDays(MaxDownloadPlaylistDays);
            
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var playlistService = scope.ServiceProvider.GetRequiredService<PlaylistService>();
            var trackService = scope.ServiceProvider.GetRequiredService<TrackService>();

            var playlist = await context.Playlists.OrderByDescending(p => p.Date).FirstOrDefaultAsync(stoppingToken);

            if (playlist == null)
            {
                return;
            }

            var playlists = new List<PlaylistDto>();

            for (var i = from; i <= to; i = i.AddDays(1))
            {
                playlists.Add(await playlistService.LoadPlaylist(i));
            }

            foreach (var track in playlists.SelectMany(p => p.Tracks).DistinctBy(t => t.Id))
            {
                await trackService.LoadTrackIfNeeded(track.Id);

                if (_env.IsProduction())
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
        }
    }
}