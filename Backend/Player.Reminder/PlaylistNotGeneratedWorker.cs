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
            //Конструктор получает настройки сервиса, поставщик сервисов, сервис для отправки сообщений в Telegram и логгер. Эти зависимости инжектируются при создании экземпляра службы.
            //             _serviceSettings: Настройки, специфичные для этой службы, загруженные из конфигурации.
            // _serviceProvider: Поставщик для получения других сервисов приложения.
            // _telegramMessageSender: Сервис для отправки сообщений через Telegram.
            // _logger: Используется для логирования событий службы.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await DoWork(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(_serviceSettings.PlaylistGenerationCheckPeriod), stoppingToken);
            }
            //ExecuteAsync(CancellationToken stoppingToken): Это основной метод фоновой службы. Он выполняется в цикле до тех пор, пока не будет запрошена остановка сервиса, вызывая метод DoWork и затем ожидая указанный в настройках период времени перед следующей итерацией.
            //         while (!stoppingToken.IsCancellationRequested): Цикл продолжается до тех пор, пока не будет запрошена остановка службы (через CancellationToken).

            // await DoWork(stoppingToken);: Выполнение основной работы службы - проверки задач на генерацию плейлистов.

            // await Task.Delay(...): Ожидание на заданный в настройках интервал времени перед следующей итерацией цикла. Время ожидания задается настройками сервиса и может быть прервано, если будет запрошена остановка службы.
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            //    Создание нового скоупа сервисов. Это нужно, чтобы получить свежий экземпляр DbContext и других сервисов.
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            //    Получение контекста базы данных PlayerContext из сервисов текущего скоупа.
            var now = DateTimeOffset.Now;
            var maxGenerationTime = TimeSpan.FromMinutes(_serviceSettings.MaxPlaylistGenerationTime);
            //    Получение текущего времени и определение максимального времени на генерацию плейлиста из настроек сервиса.
            var unFinishedPlaylistTasks = await context.Tasks
                .Where(t => t.Type == TaskType.PlaylistGeneration && !t.IsFinished)
                .ToListAsync(cancellationToken);
            //    Запрос к базе данных для получения списка задач по генерации плейлистов, которые еще не завершены.
            unFinishedPlaylistTasks = unFinishedPlaylistTasks.Where(t => now - t.RegisterDate > maxGenerationTime)
                .ToList();
            //    Фильтрация задач, время создания которых превысило максимально допустимое для генерации.
            if (!unFinishedPlaylistTasks.Any())
            {
                return;
            }
            //    Если таких задач нет, метод завершает выполнение.
            var adverts = await context.Adverts
                .Include(a => a.Uploader)
                .Where(a => unFinishedPlaylistTasks.Select(t => t.SubjectId).Contains(a.Id))
                .ToListAsync(cancellationToken);
            //    Получение рекламных объявлений из базы данных, связанных с незавершенными задачами по генерации плейлистов.
            var tasks = unFinishedPlaylistTasks.Select(t =>
            {
                var advert = adverts.Single(a => a.Id == t.SubjectId);
                if (!advert.Uploader.TelegramChatId.HasValue)
                {
                    //TODO написать кому то другому
                }

                return _telegramMessageSender.SendTextMessageAsync(advert.Uploader.TelegramChatId,
                    $"Проблема с генерацией рекламы {advert.Name}, обратитесь к администратору", cancellationToken: cancellationToken);
                //    Подготовка списка задач асинхронной отправки сообщений в Telegram для каждого необработанного объявления. Если у загрузившего нет TelegramChatId, следует добавить логику для обработки этой ситуации (обозначено как TODO).
            });

            await Task.WhenAll(tasks);
            //    Ожидание завершения всех задач отправки сообщений в Telegram.
            //         DoWork(CancellationToken cancellationToken): Метод, в котором выполняется основная логика службы:

            // Создается новый облачный сервис для изоляции зависимостей.
            // Извлекается контекст базы данных PlayerContext.
            // Получает текущее время и определяет максимально допустимое время генерации плейлиста.
            // Запрашивает из базы данных задачи по генерации плейлистов, которые не были завершены.
            // Фильтрует задачи, превышающие максимальное время генерации.
            // Если такие задачи существуют, запрашивает информацию об объявлениях, связанных с этими задачами.
            // Для каждого не завершенного вовремя задания отправляет сообщение через Telegram ответственному лицу (uploader) об этой проблеме.
            // Ждет завершения всех асинхронных операций отправки сообщений.
        }
    }
    //Эта служба обеспечивает мониторинг процесса генерации плейлистов и уведомляет ответственных лиц в случае возникновения проблем, что помогает вовремя реагировать на сбои в автоматизированных процессах.
}

// Да, ваша фоновая служба PlaylistNotGeneratedWorker предназначена для мониторинга и уведомления об ошибках в задачах по генерации плейлистов, которые не были завершены в установленное время. Позвольте мне разъяснить каждую строку:

//     using var scope = _serviceProvider.CreateScope(); - создает новый скоуп сервисов. Это необходимо для получения нового экземпляра DbContext, который должен использоваться и удаляться в рамках одного блока кода.

//     var context = scope.ServiceProvider.GetRequiredService<PlayerContext>(); - получает контекст базы данных из сервис-провайдера, который нужен для доступа к таблицам и данным в базе.

//     var now = DateTimeOffset.Now; - сохраняет текущее время, которое будет использоваться для вычисления тех задач, которые превысили установленное время на выполнение.

//     var maxGenerationTime = TimeSpan.FromMinutes(_serviceSettings.MaxPlaylistGenerationTime); - определяет максимально допустимое время для генерации плейлиста, исходя из настроек сервиса.

//     Получает из базы данных список задач по генерации плейлистов, которые не были завершены:

//     csharp

// var unFinishedPlaylistTasks = await context.Tasks
//     .Where(t => t.Type == TaskType.PlaylistGeneration && !t.IsFinished)
//     .ToListAsync(cancellationToken);

// Фильтрует полученный список задач, оставляя только те, время генерации которых превысило допустимый лимит:

// csharp

// unFinishedPlaylistTasks = unFinishedPlaylistTasks.Where(t => now - t.RegisterDate > maxGenerationTime)
//     .ToList();

// Проверяет, остались ли после фильтрации незавершенные задачи. Если нет, метод возвращает управление и не выполняет дальнейшие действия.

// Если задачи все-таки есть, делает запрос в базу данных для получения информации об объявлениях, связанных с этими задачами:

// csharp

//     var adverts = await context.Adverts
//         .Include(a => a.Uploader)
//         .Where(a => unFinishedPlaylistTasks.Select(t => t.SubjectId).Contains(a.Id))
//         .ToListAsync(cancellationToken);

//     Затем для каждой незавершенной задачи формирует асинхронный запрос на отправку уведомления через Telegram соответствующему пользователю (загрузчику объявления). Если у пользователя нет TelegramChatId, то код под комментарием //TODO написать кому то другому предполагает действие, которое нужно предпринять, если контактная информация пользователя недоступна.

//     После того как для всех незавершенных задач сформированы асинхронные запросы на отправку уведомлений, метод await Task.WhenAll(tasks); ожидает завершения всех этих асинхронных операций.

// Таким образом, ваш код действительно предназначен для мониторинга ситуаций, когда плейлисты не были сгенерированы в установленное время, и информирования об этом соответствующих лиц.