using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Player.DataAccess;
using Player.Domain;
using Player.Services.Abstractions;

namespace Player.Reminder
{
    public class PlaylistNotGeneratedWorker : BackgroundService
    {
        private readonly ServiceSettings _serviceSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITelegramMessageSender _telegramMessageSender;
        private readonly ILogger<PlaylistNotGeneratedWorker> _logger;

        public PlaylistNotGeneratedWorker(IOptions<ServiceSettings> options, 
            IServiceProvider serviceProvider,
            ITelegramMessageSender telegramMessageSender,
            ILogger<PlaylistNotGeneratedWorker> logger)
        {
            _serviceSettings = options.Value;
            _serviceProvider = serviceProvider;
            _telegramMessageSender = telegramMessageSender;
            _logger = logger;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await DoWork(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(_serviceSettings.PlaylistGenerationCheckPeriod), stoppingToken);
            }
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var now = DateTimeOffset.Now;
            var maxGenerationTime = TimeSpan.FromMinutes(_serviceSettings.MaxPlaylistGenerationTime);
            
            var unFinishedPlaylistTasks = await context.Tasks
                .Where(t => t.Type == TaskType.PlaylistGeneration && !t.IsFinished)
                .ToListAsync(cancellationToken);

            unFinishedPlaylistTasks = unFinishedPlaylistTasks.Where(t => now - t.RegisterDate > maxGenerationTime)
                .ToList();
            
            if (!unFinishedPlaylistTasks.Any())
            {
                return;
            }

            var adverts = await context.Adverts
                .Include(a => a.Uploader)
                .Where(a => unFinishedPlaylistTasks.Select(t => t.SubjectId).Contains(a.Id))
                .ToListAsync(cancellationToken);

            var tasks = unFinishedPlaylistTasks.Select(t =>
            {
                var advert = adverts.Single(a => a.Id == t.SubjectId);

                if (!advert.Uploader.TelegramChatId.HasValue)
                {
                    //TODO написать кому то другому
                }

                return _telegramMessageSender.SendTextMessageAsync(advert.Uploader.TelegramChatId,
                    $"Проблема с генерацией рекламы {advert.Name}, обратитесь к администратору", cancellationToken: cancellationToken);
            });

            await Task.WhenAll(tasks);
        }
    }
}