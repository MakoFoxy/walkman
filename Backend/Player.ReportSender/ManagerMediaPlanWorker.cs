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
            //Принимает логгер, поставщика услуг, генератор отчетов и конфигурацию через dependency injection. Эти зависимости сохраняются в приватных полях класса для дальнейшего использования.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var timeLeft =
                    DateTime.Today.Add(TimeSpan.Parse(_configuration.GetValue<string>("Player:ReportTime"))) -
                    DateTime.Now;
                //Рассчитывается время до следующего запланированного времени создания отчета.

                if (timeLeft < TimeSpan.Zero)
                {
                    timeLeft = timeLeft.Add(TimeSpan.FromDays(1));
                }

                var nextWakeUpTime = DateTime.Now.Add(timeLeft);

                _logger.LogInformation("Worker wake up at {Time}", nextWakeUpTime);
                await Task.Delay(timeLeft, stoppingToken);
                _logger.LogInformation("Worker woke up");
                //Ожидается это время.
                try
                {
                    //Вызывается метод DoWork, где выполняется основная логика.
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
            using var scope = _serviceProvider.CreateScope(); //Создается новый скоуп зависимостей.
            await using var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var telegramMessageSender = scope.ServiceProvider.GetRequiredService<ITelegramMessageSender>(); // Загружаются все менеджеры, имеющие Telegram ID, и связанные с ними объекты.
            var users = await context.Managers
                .Include(m => m.User.Objects)
                .ThenInclude(o => o.Object)
                .Where(m => m.User.TelegramChatId.HasValue)
                .ToListAsync(stoppingToken);

            var allObjects = users.SelectMany(u => u.User.Objects).Select(o => o.Object);

            var yesterday = DateTime.Today.AddDays(-1);

            var adHistories = await context.AdHistories //Загружается история рекламных объявлений для всех объектов за вчерашний день.
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
                        //Фильтруются истории рекламных объявлений, связанные с текущим объектом.
                        var tracks = new Tracks();
                        tracks.AddAdverts(histories);

                        if (tracks.Adverts.Count == 0)
                        {
                            //Если объявлений нет, логируется соответствующее сообщение и пропускается отправка отчета по этому объекту.
                            _logger.LogInformation("Sending mediaplan to manager {ManagerId} skipped in object {ObjectId}", manager.Id, o.Id);
                            continue;
                        }

                        var generatorResult = await _reportGenerator.Generate(new AdminMediaPlanPdfReportModel
                        {
                            //Если объявления есть, генерируется отчет, и он отправляется менеджеру через Telegram.
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
                    //    Код предполагает, что отчеты создаются за вчерашний день.
                    // Используется интерфейс IReportGenerator для генерации отчетов. Это абстракция, позволяющая генерировать отчеты различных типов.
                    // Отправка отчетов в Telegram осуществляется через ITelegramMessageSender. Это позволяет менеджерам получать актуальную информацию прямо в мессенджер.
                    // Используется асинхронное программирование для работы с базой данных и отправки сообщений без блокировки основного потока.
                }
            }
        }
    }
}