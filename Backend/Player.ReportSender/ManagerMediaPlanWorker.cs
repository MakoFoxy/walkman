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
    { //Класс ManagerMediaPlanWorker, наследующий BackgroundService, предназначен для асинхронной генерации и отправки отчетов по медиаплану менеджерам через Telegram. Вот подробное описание того, что происходит в коде:
      //    Конструктор принимает четыре параметра: ILogger, IServiceProvider, IReportGenerator, и IConfiguration, которые передаются через Dependency Injection. Эти зависимости сохраняются в приватных полях для использования во всем классе.
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
        { //    Этот метод вычисляет время до следующего запланированного создания отчета и ожидает это время. При наступлении запланированного времени вызывается метод DoWork, который выполняет основную логику службы. Этот код относится к методу ExecuteAsync фоновой службы в ASP.NET Core и содержит логику для циклического выполнения задачи по расписанию. Вот пошаговый разбор того, что происходит в данном коде:
            while (!stoppingToken.IsCancellationRequested)
            {//Этот цикл будет продолжаться до тех пор, пока не будет запрошена остановка сервиса через CancellationToken. stoppingToken.IsCancellationRequested возвращает true, когда службе необходимо остановить выполнение, например, при остановке приложения или перезагрузке сервера.
                var timeLeft =
                    DateTime.Today.Add(TimeSpan.Parse(_configuration.GetValue<string>("Player:ReportTime"))) -
                    DateTime.Now; //DateTime.Today возвращает текущую дату без времени.
                                  //Рассчитывается время до следующего запланированного времени создания отчета.
                                  //_configuration.GetValue<string>("Player:ReportTime") извлекает из конфигурации время, в которое должен быть сгенерирован отчёт. TimeSpan.Parse преобразует это время из строки в TimeSpan. Вычитание DateTime.Now из расчетного времени дает timeLeft — время, которое осталось до момента запуска.
                if (timeLeft < TimeSpan.Zero)
                {
                    timeLeft = timeLeft.Add(TimeSpan.FromDays(1));
                } //Если расчет показывает, что timeLeft меньше нуля, это означает, что время запуска уже прошло в текущем дне. Следовательно, к timeLeft добавляется один день, чтобы следующий запуск произошел в то же время на следующий день.

                var nextWakeUpTime = DateTime.Now.Add(timeLeft); //DateTime.Now.Add(timeLeft) рассчитывает точное время следующего запуска.

                // Логируется запланированное время пробуждения работника.

                _logger.LogInformation("Worker wake up at {Time}", nextWakeUpTime);
                await Task.Delay(timeLeft, stoppingToken); //Task.Delay(timeLeft, stoppingToken) ожидает указанное время timeLeft, позволяя асинхронно приостановить выполнение, при этом можно прервать ожидание, если будет активирован stoppingToken. После завершения задержки логируется факт пробуждения.
                _logger.LogInformation("Worker woke up");
                //Ожидается это время.
                try
                {
                    //Вызывается метод DoWork, где выполняется основная логика.
                    await DoWork(stoppingToken); //    Метод создает новый скоуп зависимостей и извлекает необходимые сервисы, такие как контекст базы данных и отправщик сообщений в Telegram.
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                    throw;
                }
            }
        }

        private async Task DoWork(CancellationToken stoppingToken)
        { //Этот блок кода описывает работу метода DoWork фоновой службы ManagerMediaPlanWorker в ASP.NET Core приложении. Задачей этого метода является генерация и отправка отчетов по рекламным кампаниям для менеджеров через Telegram. Рассмотрим подробнее каждую часть этого метода:
            using var scope = _serviceProvider.CreateScope(); //Создается новый скоуп зависимостей. Создается новый скоуп зависимостей, чтобы изолировать жизненный цикл сервисов, используемых в данной операции.
            await using var context = scope.ServiceProvider.GetRequiredService<PlayerContext>(); //Из контейнера зависимостей извлекаются PlayerContext для доступа к базе данных и ITelegramMessageSender для отправки сообщений в Telegram.
            var telegramMessageSender = scope.ServiceProvider.GetRequiredService<ITelegramMessageSender>(); // Загружаются все менеджеры, имеющие Telegram ID, и связанные с ними объекты.
            var users = await context.Managers //Загружаются данные о менеджерах, у которых указан Telegram ID, а также данные об объектах, связанных с этими менеджерами.
                .Include(m => m.User.Objects)
                .ThenInclude(o => o.Object)
                .Where(m => m.User.TelegramChatId.HasValue)
                .ToListAsync(stoppingToken);

            var allObjects = users.SelectMany(u => u.User.Objects).Select(o => o.Object); //Формируется список всех объектов для последующего поиска связанных рекламных историй.

            var yesterday = DateTime.Today.AddDays(-1);

            var adHistories = await context.AdHistories //Выбираются истории рекламных объявлений за вчерашний день для всех этих объектов.
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
                        //Фильтруются истории рекламных объявлений, связанные с текущим объектом.Для каждого менеджера и связанного объекта проверяются рекламные истории.
                        var tracks = new Tracks();
                        tracks.AddAdverts(histories);

                        if (tracks.Adverts.Count == 0)
                        {
                            //Если объявлений нет, логируется соответствующее сообщение и пропускается отправка отчета по этому объекту.
                            _logger.LogInformation("Sending mediaplan to manager {ManagerId} skipped in object {ObjectId}", manager.Id, o.Id);
                            continue;
                        }
                        //Если рекламные истории есть, генерируется PDF-отчет и отправляется через Telegram. Это делается асинхронно, что позволяет эффективно использовать системные ресурсы
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
                    {//В случае ошибок они логируются, и информация об ошибке включает идентификаторы пользователя и объекта.
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