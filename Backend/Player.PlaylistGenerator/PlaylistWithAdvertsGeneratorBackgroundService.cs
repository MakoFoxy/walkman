using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Player.DataAccess;
using Player.Domain;
using Player.Services.Abstractions;

namespace Player.PlaylistGenerator
{
    public class PlaylistWithAdvertsGeneratorBackgroundService : BackgroundService
    {
        private readonly ILogger<PlaylistWithAdvertsGeneratorBackgroundService> _logger;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private bool _playlistGenerationInProgress;
        private bool _generatingTimeTooLong;

        public PlaylistWithAdvertsGeneratorBackgroundService(ILogger<PlaylistWithAdvertsGeneratorBackgroundService> logger, IServiceProvider services, IConfiguration configuration)
        {
            _logger = logger;
            _services = services;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var value = _configuration.GetValue<int>("Player:GenerationPeriodInMinutes");

            while (!stoppingToken.IsCancellationRequested)
            {
                await AnalyzePlaylists();
                await Task.Delay(TimeSpan.FromMinutes(value), stoppingToken);
            }
        }

        public async Task AnalyzePlaylists()
        {
            try
            {
                _logger.LogInformation($"{nameof(PlaylistWithAdvertsGeneratorBackgroundService)} started");
                if (_playlistGenerationInProgress)
                {
                    _generatingTimeTooLong = true;
                    _logger.LogInformation($"{nameof(PlaylistWithAdvertsGeneratorBackgroundService)} exited because generating in progress");
                    return;
                }

                _playlistGenerationInProgress = true;

                await AnalyzePlaylistsInternal();
                _logger.LogInformation($"{nameof(PlaylistWithAdvertsGeneratorBackgroundService)} finished");
                _playlistGenerationInProgress = false;
            }
            catch (Exception e)
            {
                _playlistGenerationInProgress = false;
                _logger.LogError(e, $"{nameof(PlaylistWithAdvertsGeneratorBackgroundService)} error");
                throw;
            }
        }
        
        private async Task AnalyzePlaylistsInternal()
        {
            _generatingTimeTooLong = false;
            List<PlaylistBasicInfo> playlistBasicInfos;

            using (var scope = _services.CreateScope())
            {
                var playerContext = scope.ServiceProvider.GetRequiredService<PlayerContext>();
                playerContext.ChangeTracker.AutoDetectChangesEnabled = false;

                var playerTasks = await playerContext.Tasks
                    .Where(t => !t.IsFinished && t.Type == TaskType.PlaylistGeneration)
                    .OrderBy(pt => pt.RegisterDate)
                    .ToListAsync();
                var advertIds = playerTasks.Select(pt => pt.SubjectId).ToList();

                var adTimes = await playerContext.AdTimes
                    .Include(at => at.Object)
                    .Where(at => advertIds.Contains(at.AdvertId))
                    .ToListAsync();

                playlistBasicInfos = playerTasks
                    .GroupJoin(adTimes.ToList(), task => task.SubjectId, time => time.AdvertId,
                        (task, times) => new PlaylistBasicInfo {PlayerTask = task, AdTimes = times.ToList()})
                    .ToList();
                
                if (!playerTasks.Any())
                {
                    _logger.LogInformation($"{nameof(PlaylistWithAdvertsGeneratorBackgroundService)} exited because templates empty");
                    return;
                }
            }

            foreach (var playlistBasicInfo in playlistBasicInfos)
            {
                foreach (var adTime in playlistBasicInfo.AdTimes)
                {
                    using (var scope = _services.CreateScope())
                    {
                        var playerContext = scope.ServiceProvider.GetRequiredService<PlayerContext>();
                        var playlistGenerator = scope.ServiceProvider.GetRequiredService<IPlaylistGenerator>();
                    
                        _logger.LogInformation($"{nameof(PlaylistWithAdvertsGeneratorBackgroundService)} generating for playlist {{playlistId}}", adTime.Id);
                        await playerContext.BeginTransactionAsync();

                        var playlist = await playerContext.Playlists
                            .Where(p => p.Object.Id == adTime.ObjectId)
                            .Where(p => p.PlayingDate == adTime.PlayDate)
                            .SingleOrDefaultAsync();

                        if (playlist != null)
                        {
                            await playerContext.DeletePlaylist(playlist.Id);
                        }
                        
                        var playlistGeneratorResult = await playlistGenerator.Generate(adTime.ObjectId, adTime.PlayDate);

                        switch (playlistGeneratorResult.Status)
                        {
                            case PlaylistGeneratorStatus.None:
                                throw new ArgumentException();
                            case PlaylistGeneratorStatus.Generated:
                            {
                                if (playerContext.Entry(playlistGeneratorResult.Playlist).State == EntityState.Detached)
                                {
                                    playerContext.Playlists.Add(playlistGeneratorResult.Playlist);
                                }
                                break;
                            }
                            case PlaylistGeneratorStatus.NotGenerated:
                                break;
                            case PlaylistGeneratorStatus.Delete:
                            {
                                playerContext.Playlists.Remove(playlistGeneratorResult.Playlist);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        
                        if (adTime == playlistBasicInfo.AdTimes.Last())
                        {
                            var playerTask = await playerContext.Tasks.SingleAsync(t => t.Id == playlistBasicInfo.PlayerTask.Id);
                            playerTask.IsFinished = true;
                            playerTask.FinishDate = DateTimeOffset.Now;
                        }
                        
                        await playerContext.SaveChangesAsync();
                        await playerContext.CommitTransactionAsync();
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            if (_generatingTimeTooLong)
            {
                await AnalyzePlaylistsInternal();
            }
        }
        
        private class PlaylistBasicInfo
        {
            public PlayerTask PlayerTask { get; set; }
            public List<AdTime> AdTimes { get; set; }
        }
    }
}