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
{//Класс PartnerMediaPlanWorker, описанный в вашем коде, представляет собой фоновую службу для ASP.NET Core приложения, которая автоматически генерирует и отправляет отчеты по медиаплану партнерам через Telegram. Давайте разберем каждую строчку кода подробнее:
    //Код описывает фоновую службу PartnerMediaPlanWorker для ASP.NET Core приложения. Эта служба предназначена для автоматической генерации и отправки отчетов по медиаплану партнерам через Telegram. Рассмотрим основные части:
    private readonly ILogger<PartnerMediaPlanWorker> _logger; //Сервис логирования, используемый для записи информации о ходе выполнения работы службы.
    private readonly IServiceProvider _serviceProvider; //Провайдер услуг, используемый для доступа к другим сервисам в приложении.
    private readonly IReportGenerator<PartnerMediaPlanPdfReportModel> _reportGenerator; //Генератор отчетов, который создает PDF документы на основе модели PartnerMediaPlanPdfReportModel.
    private readonly IConfiguration _configuration; //Конфигурация приложения, откуда можно извлекать настройки, такие как время отправки отчетов.

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
        while (!stoppingToken.IsCancellationRequested) //Цикл продолжается до тех пор, пока не будет запрошена остановка службы.
        {
            var timeLeft =
                DateTime.Today.Add(TimeSpan.Parse(_configuration.GetValue<string>("Player:ReportTime"))) -
                DateTime.Now;
            //Вычисляет время до следующего заданного времени отправки отчетов.  Вычисляет время следующего запуска службы на основе конфигурации.
            if (timeLeft < TimeSpan.Zero)
            {//Если время уже прошло, добавляет 24 часа, чтобы запланировать следующий запуск на следующий день.
                timeLeft = timeLeft.Add(TimeSpan.FromDays(1));
            }

            var nextWakeUpTime = DateTime.Now.Add(timeLeft);
            //Ждет до этого момента.
            _logger.LogInformation("Worker wake up at {Time}", nextWakeUpTime);
            await Task.Delay(timeLeft, stoppingToken); // Ожидает до момента запуска.
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
        using var scope = _serviceProvider.CreateScope(); //Создает новый скоуп, который изолирует зависимости для текущего цикла работы.
        await using var context = scope.ServiceProvider.GetRequiredService<PlayerContext>(); //Извлекает необходимые сервисы из скоупа, включая контекст базы данных PlayerContext и сервис отправки сообщений ITelegramMessageSender. Получает контекст базы данных из DI контейнера.
        var telegramMessageSender = scope.ServiceProvider.GetRequiredService<ITelegramMessageSender>(); //Получает сервис для отправки сообщений в Telegram.

        var clients = await context.Clients
            .Include(c => c.Organization)
            .Include(m => m.User.Objects)
            .ThenInclude(o => o.Object)
            .ThenInclude(o => o.City)
            .Where(m => m.User.TelegramChatId.HasValue)
            .Where(c => c.User.Role.RolePermissions.Any(rp => rp.Permission.Code == Permission.PartnerAccessToObject))
            .ToListAsync(stoppingToken);
        //Запрашивает из базы данных список клиентов, у которых есть доступ к объектам и установлен Telegram ID. Запрашивает список клиентов из базы данных.
        var allObjects = clients.SelectMany(u => u.User.Objects).Select(o => o.Object); //clients предположительно содержит список клиентов, каждый из которых связан с одним или несколькими объектами через свойство User.Objects.

        var yesterday = DateTime.Today.AddDays(-1); //Эта строка вычисляет дату, которая была один день назад от текущей даты. Это делается для того, чтобы запросить данные за вчерашний день.

        var adHistories = await context.AdHistories //context.AdHistories вероятно представляет собой таблицу или коллекцию историй рекламных объявлений в базе данных.
            .Include(ah => ah.Advert)
            .Include(ah => ah.Object)
            .ThenInclude(o => o.City) //используются для загрузки связанных данных. Первый Include загружает связанные рекламные объявления, а второй — связанные объекты и их города. Это предотвращает проблему N+1 запросов и улучшает производительность, загружая все необходимые данные за один запрос.
            .Where(ah => allObjects.Contains(ah.Object) && ah.End.Date == yesterday) //Where(ah => allObjects.Contains(ah.Object) && ah.End.Date == yesterday) фильтрует истории реклам, выбирая те, которые относятся к объектам из списка allObjects и закончились вчера.
            .ToListAsync(stoppingToken); //В целом, этот код предназначен для получения списка историй рекламных объявлений для определенных объектов и за определенный день, что может быть использовано для последующего анализа или отчетности.
        //Выбирает рекламные истории для всех связанных объектов за предыдущий день.
        foreach (var client in clients) //Для каждого клиента выполняет генерацию и отправку отчетов.
        {
            //Для каждого клиента и его объектов генерирует отчеты. Если рекламных историй нет, процесс пропускается.
            _logger.LogInformation("Sending mediaplan to partner {ClientId}", client.Id);
            foreach (var o in client.User.Objects.Select(ob => ob.Object)) //    Итерация по всем объектам, связанным с пользователем клиента. client.User.Objects.Select(ob => ob.Object) выбирает из коллекции объекты, с которыми ассоциирован клиент.
            {
                try
                {
                    var histories = adHistories.Where(ah => ah.Object == o).ToList(); //Получает все рекламные истории, связанные с текущим объектом o.

                    var tracks = new Tracks(); //Создает экземпляр Tracks, который, вероятно, является контейнером для рекламных записей.
                    tracks.AddAdverts(histories); //Добавляет извлеченные рекламные истории в tracks.

                    if (tracks.Adverts.Count == 0) //Пропускает генерацию отчета, если нет данных.
                    {
                        _logger.LogInformation("Sending mediaplan to partner {ClientId} skipped in object {ObjectId}", client.Id, o.Id);
                        continue; //    Если рекламных записей нет (tracks.Adverts.Count == 0), то процесс генерации отчета для данного объекта пропускается, и информация об этом записывается в лог.
                    }
                    //Генерирует отчеты с помощью reportGenerator.
                    var generatorResult = await _reportGenerator.Generate(new PartnerMediaPlanPdfReportModel //Генерирует PDF отчет.
                    { //Запускает асинхронную генерацию PDF отчета с помощью reportGenerator. В параметрах передаются данные об объекте, дате и клиенте, а также о рекламных историях, относящихся к каждому объекту.
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

                    await telegramMessageSender.SendReport(client.User.TelegramChatId!.Value, //Отправляет отчет в Telegram.
                    //Отправляет сгенерированные отчеты через Telegram, записывает информацию об успехе или ошибке в логи.
                        generatorResult.Report,
                        $"{generatorResult.FileName}.{generatorResult.FileType}",
                        $"Доброе утро {client.User.SecondName} {client.User.LastName}, отчет по Вашему объекту {o.Name} за {yesterday:dd.MM.yyyy}"); //Сообщение сопровождается приветствием и информацией о том, за какой период предоставлен отчет.
                    _logger.LogInformation("Mediaplan was sent to partner {ClientId}", client.Id); //Логирует успешную отправку или ошибку.     Записывает в лог информацию о том, что отчет был успешно отправлен.
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error with partner {UserId} on {ObjectId}", client.User.Id, o.Id); //    В случае возникновения исключения во время выполнения любой из операций, ошибка логируется, предоставляя информацию о клиенте и объекте, где произошла ошибка.
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