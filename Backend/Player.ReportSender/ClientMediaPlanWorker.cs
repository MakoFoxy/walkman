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

namespace Player.ReportSender
{
    public class ClientMediaPlanWorker : BackgroundService
    {
        private readonly ILogger<ClientMediaPlanWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IReportGenerator<ClientMediaPlanPdfReportModel> _reportGenerator;
        private readonly IConfiguration _configuration;

        public ClientMediaPlanWorker(
            ILogger<ClientMediaPlanWorker> logger,
            IServiceProvider serviceProvider,
            IReportGenerator<ClientMediaPlanPdfReportModel> reportGenerator,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _reportGenerator = reportGenerator;
            _configuration = configuration;
            //Конструктор ClientMediaPlanWorker инициализирует объекты, которые будут использоваться в работе службы: логгер (ILogger), поставщик сервисов (IServiceProvider), генератор отчетов (IReportGenerator<ClientMediaPlanPdfReportModel>) и конфигурацию (IConfiguration).
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
                    //TODO Это было бы не плохо делать параллельно
                    await DoWork(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                    throw;
                }
            }
            //Этот метод является основной точкой входа для фоновой службы. Он выполняется в цикле до тех пор, пока служба активна (не получила сигнал о завершении работы). В каждой итерации цикла:

            // Рассчитывается время до следующего запуска сервиса на основе настройки времени из конфигурации (Player:ReportTime).
            // Служба "спит" до наступления запланированного времени.
            // После пробуждения пытается выполнить метод DoWork, который содержит логику создания и отправки медиапланов. Любые исключения в этом процессе логируются.
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            //Создает новый скоуп зависимостей, что позволяет использовать PlayerContext и другие сервисы в локальном контексте.
            await using var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var telegramMessageSender = scope.ServiceProvider.GetRequiredService<ITelegramMessageSender>();

            var clients = await context.Clients
                .Include(c => c.Organization)
                .Include(m => m.User.Objects)
                .ThenInclude(o => o.Object)
                .ThenInclude(o => o.City)
                .Where(m => m.User.TelegramChatId.HasValue)
                .Where(c => !c.User.Role.RolePermissions.Any(rp => rp.Permission.Code == Permission.PartnerAccessToObject))
                .ToListAsync(stoppingToken);
            //Запрашивает из базы данных список клиентов, у которых есть действующий Telegram чат ID и которые не имеют доступа к объекту в роли партнера.
            var allObjects = clients.SelectMany(u => u.User.Objects).Select(o => o.Object);

            var yesterday = DateTime.Today.AddDays(-1);

            var adHistories = await context.AdHistories
                .Include(ah => ah.Advert)
                .Include(ah => ah.Object)
                .ThenInclude(o => o.City)
                .Where(ah => allObjects.Contains(ah.Object) && ah.End.Date == yesterday)
                .ToListAsync(stoppingToken);
            //Получает список всех рекламных историй для объектов, связанных с этими клиентами, за вчерашний день.
            foreach (var client in clients)
            {
                //Для каждого клиента генерируется и отправляется отчет по каждому объекту, если для этого объекта были рекламные истории. Если нет рекламных историй для конкретного объекта, отправка отчета пропускается.
                _logger.LogInformation("Sending mediaplan to client {ClientId}", client.Id);
                foreach (var o in client.User.Objects.Select(ob => ob.Object))
                {
                    try
                    {
                        var histories = adHistories.Where(ah => ah.Object == o).ToList();

                        var tracks = new Tracks();
                        tracks.AddAdverts(histories);

                        if (tracks.Adverts.Count == 0)
                        {
                            _logger.LogInformation("Sending mediaplan to client {ClientId} skipped in object {ObjectId}", client.Id, o.Id);
                            continue;
                        }

                        var generatorResult = await _reportGenerator.Generate(new ClientMediaPlanPdfReportModel
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
                        //Используя сервис для отправки сообщений в Telegram, отчет в формате PDF отправляется клиенту.
                            generatorResult.Report,
                            $"{generatorResult.FileName}.{generatorResult.FileType}",
                            $"Доброе утро {client.User.SecondName} {client.User.LastName}, Ваш отчет по объекту {o.Name} за {yesterday:dd.MM.yyyy}");
                        _logger.LogInformation("Mediaplan was sent to client {ClientId}", client.Id);
                    }
                    catch (Exception e)
                    {
                        //В случае возникновения ошибки в процессе генерации или отправки отчета, информация об ошибке логируется.
                        _logger.LogError(e, "Error with client {UserId} on {ObjectId}", client.User.Id, o.Id);
                    }
                }
                //Весь процесс предназначен для автоматизации уведомления клиентов о рекламных активностях на их объектах, предоставляя им подробные отчеты каждый день.
            }
        }
    }
}