using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Helpers;
using MarketRadio.Player.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Player.ClientIntegration;

namespace MarketRadio.Player.Workers    
{
    public class PlaylistWatcherWorker : PlayerBackgroundServiceBase
    {
        private readonly PlayerStateManager _stateManager;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Bus _bus;
        private readonly Random _random = new();
        
        private bool _lastTickIsWorkTime;

        public PlaylistWatcherWorker(PlayerStateManager stateManager,
            ILogger<PlaylistWatcherWorker> logger,
            IServiceProvider serviceProvider,
            Bus bus) : base(stateManager)
        {
            _stateManager = stateManager;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _bus = bus;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await WaitForPlaylist(stoppingToken);
            await ReadBanList(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //TODO рассмотреть этот подход к 24-часовым объектам
                    if (_stateManager.IsOnline)
                    {
                        await WatchOnActualPlaylist();
                    }

                    await WatchOnBannedTrack(stoppingToken);
                    await WatchOnActualTrack(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                }

                await Task.Delay(TimeSpan.FromSeconds(.5), stoppingToken);
            }
        }

        private async Task ReadBanList(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var bannedTracks = await context.BanLists.Select(bl => bl.MusicTrackId).ToListAsync(stoppingToken);
            _stateManager.BannedTracks.AddRange(bannedTracks);
        }

        private async Task WatchOnBannedTrack(CancellationToken stoppingToken)
        {
            if (_stateManager.CurrentTrack == null)
            {
                return;
            }

            if (_stateManager.BannedTracks.Contains(_stateManager.CurrentTrack.Id))
            {
                _logger.LogInformation("Track {@Track} in ban list, changing to random track", _stateManager.CurrentTrack);
                var musicList = _stateManager.Playlist!.Tracks
                    .Where(t => t.Type == Track.Music && t.Id != _stateManager.CurrentTrack.Id)
                    .ToList();

                var randomMusicTrack = musicList[_random.Next(0, musicList.Count)];
                _logger.LogInformation("Track {@Track} is selected to replace track {@BannedTrack}", randomMusicTrack, randomMusicTrack);
                await ChangeCurrentTrack(randomMusicTrack);
            }
        }

        private async Task WatchOnActualPlaylist()
        {
            if (_stateManager.Playlist == null)
            {
                return;
            }
            
            if (_stateManager.Playlist.Date.Date != DateTime.Today)
            {
                using var scope = _serviceProvider.CreateScope();
                var playlistService = scope.ServiceProvider.GetRequiredService<PlaylistService>();
                var playlist = await playlistService.LoadPlaylist(DateTime.Today);
                await _stateManager.ChangePlaylist(playlist);
            }
        }

        private async Task WatchOnActualTrack(CancellationToken stoppingToken)
        {
            if (_stateManager.Object == null)
            {
                return;
            }
            
            if (_stateManager.Object.FreeDays.Contains(DateTime.Now.DayOfWeek))
            {
                return;
            }

            if (_lastTickIsWorkTime && !NowTheWorkingTime)
            {
                _logger.LogInformation("Stop playing");
                await _bus.StopPlaying();
                _lastTickIsWorkTime = false;
            }

            if (!NowTheWorkingTime)
            {
                return;
            }

            _lastTickIsWorkTime = true;

            var playlistTracks = _stateManager.Playlist?.Tracks ?? ArraySegment<TrackDto>.Empty;
            var tracks = playlistTracks
                .Where(t => t.PlayingDateTime.TimeOfDay <= DateTime.Now.TimeOfDay && 
                            t.PlayingDateTime.AddSeconds(t.Length).TimeOfDay > DateTime.Now.TimeOfDay)
                .Where(t => !_stateManager.BannedTracks.Contains(t.Id))
                .ToList();
            
            TrackDto? track = null;
            
            if (!tracks.Any())
            {
                return;
            }

            if (tracks.Count == 1)
            {
                track = tracks[0];
            }

            if (tracks.Any(t => t.Type == Track.Advert))
            {
                track = tracks.First(t => t.Type == Track.Advert);
            }

            if (NowTheWorkingTime && track == null)
            {
                _logger.LogWarning("Track is null in work time {CurrentTime}", DateTime.Now);
            }

            if (track == null)
            {
                return;
            }

            if (_stateManager.CurrentTrack?.UniqueId != track.UniqueId)
            {
                if (!File.Exists(Path.Combine(DefaultLocations.TracksPath, track.UniqueName)))
                {
                    _logger.LogWarning("Track {TrackName} not exists", track.UniqueName);
                }
                
                await ChangeCurrentTrack(track);

                _logger.LogInformation(
                    "Current track changed {@Track} next track start time {NextTrackStartTime}",
                    track, _stateManager.NextTrack?.PlayingDateTime);
            }
        }

        private async Task ChangeCurrentTrack(TrackDto track)
        {
            await _stateManager.ChangeCurrentTrack(track);
        }
    }
}