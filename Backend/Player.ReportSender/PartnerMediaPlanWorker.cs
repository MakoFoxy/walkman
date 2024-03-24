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
using Player.Domain;
using Player.Services.Abstractions;
using Player.Services.Report.Abstractions;
using Player.Services.Report.MediaPlan;
using Client = Player.Services.Report.Abstractions.Client;

namespace Player.ReportSender;

public class PartnerMediaPlanWorker : BackgroundService
{
    //Код описывает фоновую службу PartnerMediaPlanWorker для ASP.NET Core приложения. Эта служба предназначена для автоматической генерации и отправки отчетов по медиаплану партнерам через Telegram. Рассмотрим основные части:
    private readonly ILogger<PartnerMediaPlanWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IReportGenerator<PartnerMediaPlanPdfReportModel> _reportGenerator;
    private readonly IConfiguration _configuration;

    public PartnerMediaPlanWorker(
        ILogger<PartnerMediaPlanWorker> logger,
        IServiceProvider serviceProvider,
        IReportGenerator<PartnerMediaPlanPdfReportModel> reportGenerator,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _reportGenerator = reportGenerator;
        _configuration = configuration;
        //Принимает и сохраняет зависимости:

        // ILogger<PartnerMediaPlanWorker> для логирования событий службы.
        // IServiceProvider для доступа к другим сервисам приложения.
        // IReportGenerator<PartnerMediaPlanPdfReportModel> для генерации PDF отчетов.
        // IConfiguration для доступа к настройкам приложения.
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var timeLeft =
                DateTime.Today.Add(TimeSpan.Parse(_configuration.GetValue<string>("Player:ReportTime"))) -
                DateTime.Now;
            //Вычисляет время до следующего заданного времени отправки отчетов.
            if (timeLeft < TimeSpan.Zero)
            {
                timeLeft = timeLeft.Add(TimeSpan.FromDays(1));
            }

            var nextWakeUpTime = DateTime.Now.Add(timeLeft);
            //Ждет до этого момента.
            _logger.LogInformation("Worker wake up at {Time}", nextWakeUpTime);
            await Task.Delay(timeLeft, stoppingToken);
            _logger.LogInformation("Worker woke up");

            try
            {
                //Пытается выполнить работу в методе DoWork. Ловит и логирует исключения, если они возникают.
                //TODO Это было бы не плохо делать параллельно
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
        //Создает новый скоуп для разрешения зависимостей (таким образом сервисы используются только во время одного цикла работы).
        using var scope = _serviceProvider.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PlayerContext>(); //Извлекает необходимые сервисы из скоупа, включая контекст базы данных PlayerContext и сервис отправки сообщений ITelegramMessageSender.
        var telegramMessageSender = scope.ServiceProvider.GetRequiredService<ITelegramMessageSender>();

        var clients = await context.Clients
            .Include(c => c.Organization)
            .Include(m => m.User.Objects)
            .ThenInclude(o => o.Object)
            .ThenInclude(o => o.City)
            .Where(m => m.User.TelegramChatId.HasValue)
            .Where(c => c.User.Role.RolePermissions.Any(rp => rp.Permission.Code == Permission.PartnerAccessToObject))
            .ToListAsync(stoppingToken);
        //Запрашивает из базы данных список клиентов, у которых есть доступ к объектам и установлен Telegram ID.
        var allObjects = clients.SelectMany(u => u.User.Objects).Select(o => o.Object);

        var yesterday = DateTime.Today.AddDays(-1);

        var adHistories = await context.AdHistories
            .Include(ah => ah.Advert)
            .Include(ah => ah.Object)
            .ThenInclude(o => o.City)
            .Where(ah => allObjects.Contains(ah.Object) && ah.End.Date == yesterday)
            .ToListAsync(stoppingToken);
        //Выбирает рекламные истории для всех связанных объектов за предыдущий день.
        foreach (var client in clients)
        {
            //Для каждого клиента и его объектов генерирует отчеты. Если рекламных историй нет, процесс пропускается.
            _logger.LogInformation("Sending mediaplan to partner {ClientId}", client.Id);
            foreach (var o in client.User.Objects.Select(ob => ob.Object))
            {
                try
                {
                    var histories = adHistories.Where(ah => ah.Object == o).ToList();

                    var tracks = new Tracks();
                    tracks.AddAdverts(histories);

                    if (tracks.Adverts.Count == 0)
                    {
                        _logger.LogInformation("Sending mediaplan to partner {ClientId} skipped in object {ObjectId}", client.Id, o.Id);
                        continue;
                    }
                    //Генерирует отчеты с помощью reportGenerator.
                    var generatorResult = await _reportGenerator.Generate(new PartnerMediaPlanPdfReportModel
                    {
                        Object = new Services.Report.Abstractions.Object
                        {
                            Name = o.Name,
                            BeginTime = o.BeginTime,
                            EndTime = o.EndTime
                        },
                        Date = yesterday,
                        ReportBegin = yesterday,
                        ReportEnd = yesterday,
                        Client = new Client
                        {
                            Name = client.Organization.Name
                        },
                        ObjectHistoryModels = adHistories.Select(ah => new ObjectHistoryModel
                        {
                            Object = ah.Object,
                            History = adHistories.Where(adh => adh.Object == ah.Object)
                        }).DistinctBy(ohm => ohm.Object.Id)
                    });

                    await telegramMessageSender.SendReport(client.User.TelegramChatId!.Value,
                    //Отправляет сгенерированные отчеты через Telegram, записывает информацию об успехе или ошибке в логи.
                        generatorResult.Report,
                        $"{generatorResult.FileName}.{generatorResult.FileType}",
                        $"Доброе утро {client.User.SecondName} {client.User.LastName}, отчет по Вашему объекту {o.Name} за {yesterday:dd.MM.yyyy}");
                    _logger.LogInformation("Mediaplan was sent to partner {ClientId}", client.Id);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error with partner {UserId} on {ObjectId}", client.User.Id, o.Id);
                }
            }
        }
        //    Служба предполагает ежедневное выполнение и ориентирована на отправку отчетов за предыдущий день.
        // Используется механизм DI для получения сервисов и работы с базой данных.
        // Используются асинхронные операции для взаимодействия с базой данных и отправки сообщений, что позволяет эффективно использовать ресурсы сервера.
        // Отчеты создаются для каждого объекта клиента, позволяя партнерам получать подробную информацию о рекламных активностях.
        // Ошибки в процессе генерации или отправки отчетов логируются, что помогает в диагностике и устранении проблем.
    }
}