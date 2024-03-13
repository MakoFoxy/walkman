using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Player.DataAccess;
using Player.Services.Abstractions;

namespace Player.PlaylistGenerator
{
    public class EmptyPlaylistGeneratorBackgroundService : BackgroundService
    {
        private readonly ILogger<EmptyPlaylistGeneratorBackgroundService> _logger;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;

        public EmptyPlaylistGeneratorBackgroundService(ILogger<EmptyPlaylistGeneratorBackgroundService> logger, IServiceProvider services, IConfiguration configuration)
        {
            _logger = logger;
            _services = services;
            _configuration = configuration;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await CreateEmptyPlaylist(stoppingToken);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var timeLeft = DateTime.Today.Add(TimeSpan.Parse(_configuration.GetValue<string>("Player:EmptyPlaylistGenerationTime"))) - DateTime.Now;

                if (timeLeft < TimeSpan.Zero)
                {
                    timeLeft = timeLeft.Add(TimeSpan.FromDays(1));
                }
                
                var nextWakeUpTime = DateTime.Now.Add(timeLeft);
                
                _logger.LogInformation("EmptyPlaylistGenerator wake up at {NextWakeUpTime}", nextWakeUpTime);
                await Task.Delay(timeLeft, stoppingToken);
                _logger.LogInformation("EmptyPlaylistGenerator woke up");
                
                await CreateEmptyPlaylist(stoppingToken);
            }
        }

        private async Task CreateEmptyPlaylist(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Empty playlist generation started");

            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            var nextDay = DateTime.Now.Date.AddDays(1);

            var objects = await context.Objects
                .Where(o => !o.Playlists.Any(p => p.PlayingDate.Date == nextDay))
                .Where(o => !context.AdTimes.Where(at => at.PlayDate == nextDay && at.Object == o).Any())
                .ToListAsync(stoppingToken);
            
            var playlistGenerator = scope.ServiceProvider.GetRequiredService<IPlaylistGenerator>();
            foreach (var @object in objects)
            {
                _logger.LogInformation("Generating empty playlist for {@Object} on {NextDay}",  @object, nextDay);
                await context.BeginTransactionAsync();
                
                var playlistGeneratorResult = await playlistGenerator.Generate(@object, nextDay);

                switch (playlistGeneratorResult.Status)
                {
                    case PlaylistGeneratorStatus.None:
                        throw new ArgumentException();
                    case PlaylistGeneratorStatus.Generated:
                    {
                        if (context.Entry(playlistGeneratorResult.Playlist).State == EntityState.Detached)
                        {
                            context.Playlists.Add(playlistGeneratorResult.Playlist);
                        }
                        break;
                    }
                    case PlaylistGeneratorStatus.NotGenerated:
                        break;
                    case PlaylistGeneratorStatus.Delete:
                    {
                        context.Playlists.Remove(playlistGeneratorResult.Playlist);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await context.SaveChangesAsync();
                await context.CommitTransactionAsync();

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            
            _logger.LogInformation("Empty playlist generation end");
        }
    }
}