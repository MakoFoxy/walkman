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
using Player.Services.Report.Abstractions;
using Player.Services.Report.MediaPlan;

namespace Player.ReportSender
{
    public class ManagerMediaPlanWorker : BackgroundService
    {
        private readonly ILogger<ManagerMediaPlanWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IReportGenerator<AdminMediaPlanPdfReportModel> _reportGenerator;
        private readonly IConfiguration _configuration;

        public ManagerMediaPlanWorker(
            ILogger<ManagerMediaPlanWorker> logger,
            IServiceProvider serviceProvider,
            IReportGenerator<AdminMediaPlanPdfReportModel> reportGenerator,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _reportGenerator = reportGenerator;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var timeLeft =
                    DateTime.Today.Add(TimeSpan.Parse(_configuration.GetValue<string>("Player:ReportTime"))) -
                    DateTime.Now;

                if (timeLeft < TimeSpan.Zero)
                {
                    timeLeft = timeLeft.Add(TimeSpan.FromDays(1));
                }

                var nextWakeUpTime = DateTime.Now.Add(timeLeft);

                _logger.LogInformation("Worker wake up at {Time}", nextWakeUpTime);
                await Task.Delay(timeLeft, stoppingToken);
                _logger.LogInformation("Worker woke up");

                try
                {
                    await DoWork(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                    throw;
                }
            }
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var telegramMessageSender = scope.ServiceProvider.GetRequiredService<ITelegramMessageSender>();

            var users = await context.Managers
                .Include(m => m.User.Objects)
                .ThenInclude(o => o.Object)
                .Where(m => m.User.TelegramChatId.HasValue)
                .ToListAsync(stoppingToken);

            var allObjects = users.SelectMany(u => u.User.Objects).Select(o => o.Object);

            var yesterday = DateTime.Today.AddDays(-1);

            var adHistories = await context.AdHistories
                .Include(ah => ah.Advert)
                .Where(ah => allObjects.Contains(ah.Object) && ah.End.Date == yesterday)
                .ToListAsync(stoppingToken);

            foreach (var manager in users)
            {
                _logger.LogInformation("Sending mediaplan to manager {ManagerId}", manager.Id);
                foreach (var o in manager.User.Objects.Select(ob => ob.Object))
                {
                    try
                    {
                        var histories = adHistories.Where(ah => ah.Object == o).ToList();

                        var tracks = new Tracks();
                        tracks.AddAdverts(histories);

                        if (tracks.Adverts.Count == 0)
                        {
                            _logger.LogInformation("Sending mediaplan to manager {ManagerId} skipped in object {ObjectId}", manager.Id, o.Id);
                            continue;
                        }
                        
                        var generatorResult = await _reportGenerator.Generate(new AdminMediaPlanPdfReportModel
                        {
                            Object = new Services.Report.Abstractions.Object
                            {
                                Name = o.Name,
                                BeginTime = o.BeginTime,
                                EndTime = o.EndTime
                            },
                            Date = yesterday,
                            Tracks = tracks,
                            ReportBegin = yesterday,
                            ReportEnd = yesterday
                        });

                        await telegramMessageSender.SendReport(manager.User.TelegramChatId!.Value,
                            generatorResult.Report,
                            $"{generatorResult.FileName}.{generatorResult.FileType}",
                            $"Доброе утро, {manager.User.SecondName}");
                        _logger.LogInformation("Mediaplan was sent to manager {ManagerId}", manager.Id);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error with {UserId} on {ObjectId}", manager.User.Id, o.Id);
                    }
                }
            }
        }
    }
}