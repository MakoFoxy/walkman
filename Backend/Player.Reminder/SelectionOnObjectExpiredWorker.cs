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
            //Конструктор получает настройки сервиса, поставщик сервисов, сервис для отправки сообщений в Telegram и логгер через dependency injection. Эти зависимости используются в дальнейшей логике сервиса.
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
                //Основной метод фоновой службы, который работает в бесконечном цикле до получения сигнала об остановке. Внутри цикла:

                // Рассчитывается время до следующего запланированного пробуждения службы на основе настроек.
                // Ожидается это время.
                // Затем вызывается метод DoWork, где выполняется основная логика службы.
            }
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var expireDate = DateTimeOffset.Now.Date.AddDays(_serviceSettings.SelectionExpiredDays);

            var selections = await context.Selections
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

            //В этом методе:

            // Создается новый облачный сервис для работы с зависимостями.
            // Из сервис-провайдера извлекается контекст базы данных PlayerContext.
            // Определяется дата, начиная с которой селекции считаются просроченными.
            // Запрашивается список просроченных селекций и связанных с ними объектов из базы данных.
            // Запрашиваются пользователи, у которых есть просроченные селекции, и которые имеют идентификатор чата Telegram для отправки уведомлений.
            // Для каждого пользователя с просроченными селекциями формируются и отправляются сообщения о каждой просроченной селекции.
        }
    }
    //Таким образом, эта служба отслеживает просроченные селекции объектов и уведомляет ответственных пользователей через Telegram, что позволяет своевременно реагировать на истечение срока действия селекций и предпринимать необходимые действия.
}

// /Термин "селекция" может иметь разные значения в зависимости от контекста. В вашем контексте, судя по коду, "селекция" вероятно относится к выборке или подборке определенных объектов или данных в системе. В контексте программного обеспечения или информационных систем, это может быть набор данных, объектов или элементов, которые были отобраны или выбраны для определенной цели.

// Например, в музыкальном приложении или сервисе, селекция может означать подборку треков, плейлист, выбранный для воспроизведения в определенное время или для конкретного объекта (например, магазина, кафе, и т.д.). Селекции могут быть сформированы на основе различных критериев: жанра, популярности, времени суток, личных предпочтений пользователя и так далее.

// В контексте вашего кода, кажется, что селекции относятся к определенным выборкам или наборам данных, связанным с объектами, и существует механизм для отслеживания их "срока действия". Когда селекция "истекает", то есть достигает определенной даты, после которой она считается устаревшей или недействительной, система должна уведомить ответственных лиц или пользователей, чтобы они могли обновить или изменить эту селекцию.