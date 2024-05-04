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

namespace Player.PlaylistGenerator
{
    public class EmptyPlaylistGeneratorBackgroundService : BackgroundService
    {
        private readonly ILogger<EmptyPlaylistGeneratorBackgroundService> _logger;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;

        public EmptyPlaylistGeneratorBackgroundService(ILogger<EmptyPlaylistGeneratorBackgroundService> logger, IServiceProvider services, IConfiguration configuration)
        {
            _logger = logger;
            _services = services;
            _configuration = configuration;
            //Конструктор: Конструктор принимает экземпляры ILogger, IServiceProvider и IConfiguration через внедрение зависимостей. Эти экземпляры используются на протяжении всей службы:
            // ILogger для ведения журнала информации, предупреждений и ошибок.
            // IServiceProvider для создания областей внедрения зависимостей.
            // IConfiguration для доступа к настройкам приложения, например, к расписанию генерации плейлистов.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await CreateEmptyPlaylist(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {//Расчет времени до следующего выполнения: Сначала определяется разница между текущим временем и временем, установленным для выполнения задачи (13:00 текущего дня).
                var timeLeft = DateTime.Today.Add(TimeSpan.Parse(_configuration.GetValue<string>("Player:EmptyPlaylistGenerationTime"))) - DateTime.Now;

                if (timeLeft < TimeSpan.Zero)
                {//Корректировка времени: Если рассчитанное время уже прошло (т.е. timeLeft < TimeSpan.Zero), то к этому времени добавляется один день, чтобы задача выполнилась в 13:00 следующего дня.
                    timeLeft = timeLeft.Add(TimeSpan.FromDays(1));
                }

                var nextWakeUpTime = DateTime.Now.Add(timeLeft);

                _logger.LogInformation("EmptyPlaylistGenerator wake up at {NextWakeUpTime}", nextWakeUpTime);
                await Task.Delay(timeLeft, stoppingToken); //Ожидание до следующего времени выполнения: Сервис ожидает до рассчитанного времени (nextWakeUpTime), используя Task.Delay(timeLeft, stoppingToken), прежде чем приступить к созданию пустого плейлиста.
                _logger.LogInformation("EmptyPlaylistGenerator woke up");

                await CreateEmptyPlaylist(stoppingToken);
                //Это основной метод, где выполняется логика фоновой задачи. Он следует циклу, который продолжается до получения сигнала остановки. Внутри цикла:
                // Вычисляется оставшееся время до следующего выполнения на основе текущего времени и настроенного расписания.
                // Происходит ожидание до времени следующего выполнения с помощью Task.Delay.
                // Вызывается метод CreateEmptyPlaylist для генерации пустых плейлистов.
            }
        }

        private async Task CreateEmptyPlaylist(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Empty playlist generation started");

            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            var nextDay = DateTime.Now.Date.AddDays(1);

            var objects = await context.Objects
                .Where(o => !o.Playlists.Any(p => p.PlayingDate.Date == nextDay))
                //Не существует плейлистов, запланированных на следующий день (!o.Playlists.Any(p => p.PlayingDate.Date == nextDay)).
                .Where(o => !context.AdTimes.Where(at => at.PlayDate == nextDay && at.Object == o).Any())
                //Не запланировано рекламное время на следующий день для этих объектов (!context.AdTimes.Where(at => at.PlayDate == nextDay && at.Object == o).Any()).
                .ToListAsync(stoppingToken);

            var playlistGenerator = scope.ServiceProvider.GetRequiredService<IPlaylistGenerator>();
            foreach (var @object in objects)
            {
                _logger.LogInformation("Generating empty playlist for {@Object} on {NextDay}", @object, nextDay);
                await context.BeginTransactionAsync();

                var playlistGeneratorResult = await playlistGenerator.Generate(@object, nextDay);

                switch (playlistGeneratorResult.Status)
                {
                    case PlaylistGeneratorStatus.None:
                        throw new ArgumentException();
                    case PlaylistGeneratorStatus.Generated:
                        {
                            if (context.Entry(playlistGeneratorResult.Playlist).State == EntityState.Detached)
                            {
                                context.Playlists.Add(playlistGeneratorResult.Playlist);
                            }
                            break;
                        }
                    case PlaylistGeneratorStatus.NotGenerated:
                        break;
                    case PlaylistGeneratorStatus.Delete:
                        {
                            context.Playlists.Remove(playlistGeneratorResult.Playlist);
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                        //Метод CreateEmptyPlaylist: Этот метод реализует логику генерации пустых плейлистов. Он выполняет следующие действия:
                        // Начинается с записи в журнал о начале процесса генерации.
                        // Создает новую область для внедрения зависимостей.
                        // Извлекает PlayerContext и IPlaylistGenerator из поставщика услуг.
                        // Отключает отслеживание изменений для повышения производительности, поскольку отслеживание не требуется для операций массового обновления.
                        // Запрашивает объекты (например, песни или медиаэлементы), которые не имеют запланированного плейлиста на следующий день и не зарезервированы для рекламы на следующий день.
                        // Для каждого объекта генерируется пустой плейлист на следующий день с использованием IPlaylistGenerator.
                        // В зависимости от результата генератора плейлистов, может быть добавлен новый плейлист, удален существующий или ничего не делать.
                        // Сохраняет изменения в базе данных и фиксирует транзакцию.
                        // Задерживает выполнение, чтобы избежать быстрого выполнения транзакций с базой данных и снизить нагрузку.
                }

                await context.SaveChangesAsync();
                await context.CommitTransactionAsync();

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }

            _logger.LogInformation("Empty playlist generation end");
        }
    }
}

//В этом коде используется Entity Framework Core для доступа к данным, паттерны асинхронного программирования и внедрение зависимостей для создания областей. Этот паттерн типичен для приложений ASP.NET Core для задач, которые должны выполняться периодически или непрерывно в фоновом режиме.