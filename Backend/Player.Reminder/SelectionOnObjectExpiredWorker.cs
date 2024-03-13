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
using Player.Services.Abstractions;

namespace Player.Reminder
{
    public class SelectionOnObjectExpiredWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITelegramMessageSender _telegramMessageSender;
        private readonly ILogger<SelectionOnObjectExpiredWorker> _logger;
        private readonly ServiceSettings _serviceSettings;

        public SelectionOnObjectExpiredWorker(IOptions<ServiceSettings> options, 
            IServiceProvider serviceProvider,
            ITelegramMessageSender telegramMessageSender,
            ILogger<SelectionOnObjectExpiredWorker> logger
            )
        {
            _serviceProvider = serviceProvider;
            _telegramMessageSender = telegramMessageSender;
            _logger = logger;
            _serviceSettings = options.Value;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var timeLeft = DateTime.Today.Add(_serviceSettings.WakeUpTime) - DateTime.Now;

                if (timeLeft < TimeSpan.Zero)
                {
                    timeLeft = timeLeft.Add(TimeSpan.FromDays(1));
                }
                
                var nextWakeUpTime = DateTime.Now.Add(timeLeft);
                
                _logger.LogInformation("Worker wake up at {Time}", nextWakeUpTime);
                await Task.Delay(timeLeft, stoppingToken);
                _logger.LogInformation("Worker woke up");
                
                await DoWork(stoppingToken);
            }
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var expireDate = DateTimeOffset.Now.Date.AddDays(_serviceSettings.SelectionExpiredDays);
            
            var selections = await  context.Selections
                .Include(s => s.Objects)
                .ThenInclude(so => so.Object)
                .Where(s => s.DateEnd < expireDate)
                .ToListAsync(cancellationToken);

            var objects = selections.SelectMany(s => s.Objects)
                .Select(so => so.Object)
                .ToList();

            var users = await context.Users
                .Include(u => u.Objects)
                .ThenInclude(uo => uo.Object)
                .Where(u => u.TelegramChatId.HasValue)
                .Where(u => u.Objects.Any(o => objects.Contains(o.Object)))
                .ToListAsync(cancellationToken);
            
            foreach (var user in users)
            {
                var objectsWithExpiredSelections = user.Objects.Select(uo => uo.Object).Intersect(objects);

                foreach (var objectWithExpiredSelection in objectsWithExpiredSelections)
                {
                    var selectionsExpired = selections.Where(s => s.Objects.Select(so => so.Object).Any(so => so == objectWithExpiredSelection))
                        .ToList();

                    foreach (var selection in selectionsExpired)
                    {
                        var days = (int)(selection.DateEnd!.Value - DateTimeOffset.Now.Date).TotalDays;

                        await _telegramMessageSender.SendSelectionExpired(user.TelegramChatId!.Value, days, selection.Name,
                            objectWithExpiredSelection.Name);
                    }
                }
            }
        }
    }
}