using System;
using System.Collections.Generic;
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

namespace Player.PlaylistGenerator
{
    public class PlaylistWithAdvertsGeneratorBackgroundService : BackgroundService
    {
        private readonly ILogger<PlaylistWithAdvertsGeneratorBackgroundService> _logger;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private bool _playlistGenerationInProgress;
        private bool _generatingTimeTooLong;

        public PlaylistWithAdvertsGeneratorBackgroundService(ILogger<PlaylistWithAdvertsGeneratorBackgroundService> logger, IServiceProvider services, IConfiguration configuration)
        {
            _logger = logger;
            _services = services;
            _configuration = configuration;
            //Конструктор: Принимает экземпляры ILogger, IServiceProvider, и IConfiguration через внедрение зависимостей, которые используются для ведения журнала, доступа к службам и конфигурации приложения соответственно.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var value = _configuration.GetValue<int>("Player:GenerationPeriodInMinutes");

            while (!stoppingToken.IsCancellationRequested)
            {
                await AnalyzePlaylists();
                await Task.Delay(TimeSpan.FromMinutes(value), stoppingToken);
            }
            //Метод ExecuteAsync: Определяет логику повторения задачи. Служба будет анализировать плейлисты через регулярные интервалы времени, заданные в конфигурации (Player:GenerationPeriodInMinutes).
        }

        public async Task AnalyzePlaylists()
        {
            try
            {
                _logger.LogInformation($"{nameof(PlaylistWithAdvertsGeneratorBackgroundService)} started");
                if (_playlistGenerationInProgress)
                {
                    _generatingTimeTooLong = true;
                    _logger.LogInformation($"{nameof(PlaylistWithAdvertsGeneratorBackgroundService)} exited because generating in progress");
                    return;
                }
                //Если генерация плейлиста уже идет, то метод возвращает управление, чтобы избежать параллельного выполнения, и устанавливает флаг _generatingTimeTooLong в true.

                _playlistGenerationInProgress = true;
                //Устанавливается флаг, указывающий, что началась генерация плейлиста.

                await AnalyzePlaylistsInternal();
                //Вызывается внутренний метод AnalyzePlaylistsInternal, который содержит логику анализа плейлистов. После выполнения метода в журнал выводится сообщение об окончании анализа, и флаг генерации сбрасывается.
                _logger.LogInformation($"{nameof(PlaylistWithAdvertsGeneratorBackgroundService)} finished");
                _playlistGenerationInProgress = false;
            }
            catch (Exception e)
            {
                _playlistGenerationInProgress = false;
                _logger.LogError(e, $"{nameof(PlaylistWithAdvertsGeneratorBackgroundService)} error");
                throw;
                //В случае возникновения исключения сбрасывается флаг генерации, записывается ошибка в журнал и исключение повторно выбрасывается.
            }
            //Метод AnalyzePlaylists: Вызывается в ExecuteAsync для проверки и обновления плейлистов. Если генерация плейлиста уже выполняется, метод возвращает управление, чтобы избежать параллельного выполнения. В противном случае он устанавливает флаг _playlistGenerationInProgress в true и начинает анализ плейлистов.
        }

        private async Task AnalyzePlaylistsInternal()
        {
            _generatingTimeTooLong = false;
            List<PlaylistBasicInfo> playlistBasicInfos;
            //Метод для внутренней логики анализа плейлистов. Сбрасывается флаг _generatingTimeTooLong, и инициализируется список для хранения базовой информации о плейлистах.

            using (var scope = _services.CreateScope())
            //Создается новая область для внедрения зависимостей (DI scope), что позволяет получить новый экземпляр контекста базы данных и других сервисов для каждой операции.
            {
                var playerContext = scope.ServiceProvider.GetRequiredService<PlayerContext>();
                //Из контейнера зависимостей получается контекст базы данных PlayerContext.  
                playerContext.ChangeTracker.AutoDetectChangesEnabled = false;
                //Отключение автоматического отслеживания изменений (AutoDetectChangesEnabled = false) используется для улучшения производительности при чтении данных, так как в данном контексте изменения в сущностях не требуются.

                var playerTasks = await playerContext.Tasks
                    .Where(t => !t.IsFinished && t.Type == TaskType.PlaylistGeneration)
                    .OrderBy(pt => pt.RegisterDate)
                    .ToListAsync();
                //Извлекается список задач по генерации плейлистов, которые еще не завершены. Эти задачи упорядочиваются по дате регистрации.
                var advertIds = playerTasks.Select(pt => pt.SubjectId).ToList();
                //Создается список идентификаторов рекламных акций, связанных с задачами на генерацию плейлистов.

                var adTimes = await playerContext.AdTimes
                    .Include(at => at.Object)
                    .Where(at => advertIds.Contains(at.AdvertId))
                    .ToListAsync();
                //Загружается список временных интервалов рекламы, соответствующих задачам по генерации плейлистов. Эти интервалы включают связанные с ними объекты (например, песни или видео).
                playlistBasicInfos = playerTasks
                    .GroupJoin(adTimes.ToList(), task => task.SubjectId, time => time.AdvertId,
                        (task, times) => new PlaylistBasicInfo { PlayerTask = task, AdTimes = times.ToList() })
                    .ToList();
                //Создается список базовой информации о плейлистах, объединяя задачи на генерацию с соответствующими интервалами рекламы.
                if (!playerTasks.Any())
                {
                    _logger.LogInformation($"{nameof(PlaylistWithAdvertsGeneratorBackgroundService)} exited because templates empty");
                    return;
                }
                //Если задач по генерации плейлистов нет, метод выводит сообщение в журнал и завершается.
            }

            foreach (var playlistBasicInfo in playlistBasicInfos)
            {
                foreach (var adTime in playlistBasicInfo.AdTimes)
                {
                    using (var scope = _services.CreateScope())
                    {
                        var playerContext = scope.ServiceProvider.GetRequiredService<PlayerContext>();
                        var playlistGenerator = scope.ServiceProvider.GetRequiredService<IPlaylistGenerator>();
                        //Для каждой базовой информации о плейлисте и каждого временного интервала рекламы создается новая область DI и извлекаются необходимые сервисы.
                        _logger.LogInformation($"{nameof(PlaylistWithAdvertsGeneratorBackgroundService)} generating for playlist {{playlistId}}", adTime.Id);
                        //Начинается новая транзакция базы данных, и в журнал выводится сообщение о начале генерации плейлиста для текущего интервала рекламы.
                        await playerContext.BeginTransactionAsync();

                        var playlist = await playerContext.Playlists
                            .Where(p => p.Object.Id == adTime.ObjectId)
                            .Where(p => p.PlayingDate == adTime.PlayDate)
                            .SingleOrDefaultAsync();
                        //Поиск существующего плейлиста для данного объекта и даты.

                        if (playlist != null)
                        {
                            await playerContext.DeletePlaylist(playlist.Id);
                            //Вызывается генератор плейлистов для создания нового плейлиста.
                        }

                        var playlistGeneratorResult = await playlistGenerator.Generate(adTime.ObjectId, adTime.PlayDate);
                        //Вызывается генератор плейлистов для создания нового плейлиста.
                        switch (playlistGeneratorResult.Status)
                        {
                            case PlaylistGeneratorStatus.None:
                                throw new ArgumentException();
                                //Если статус результата генерации равен None, это считается ошибочным состоянием, и генерируется исключение ArgumentException.
                            case PlaylistGeneratorStatus.Generated:
                                {
                                    if (playerContext.Entry(playlistGeneratorResult.Playlist).State == EntityState.Detached)
                                    {
                                        playerContext.Playlists.Add(playlistGeneratorResult.Playlist);
                                    }
                                    break;
                                    //Если статус результата генерации равен Generated, проверяется, не отсоединен ли плейлист от контекста (то есть не трекается ли он). Если это так, плейлист добавляется в контекст для последующего сохранения в базе данных.
                                }
                            case PlaylistGeneratorStatus.NotGenerated:
                                break;
                                //Если статус равен NotGenerated, ничего не делается, и обработка переходит к следующему блоку кода.
                            case PlaylistGeneratorStatus.Delete:
                                {
                                    playerContext.Playlists.Remove(playlistGeneratorResult.Playlist);
                                    break;
                                    //Если статус равен Delete, указанный плейлист удаляется из контекста базы данных.
                                }
                            default:
                                throw new ArgumentOutOfRangeException();
                                //Если встретился неизвестный статус, генерируется исключение ArgumentOutOfRangeException, так как это непредвиденная ситуация.
                        }

                        if (adTime == playlistBasicInfo.AdTimes.Last())
                        {
                            var playerTask = await playerContext.Tasks.SingleAsync(t => t.Id == playlistBasicInfo.PlayerTask.Id);
                            playerTask.IsFinished = true;
                            playerTask.FinishDate = DateTimeOffset.Now;
                            //Проверяется, является ли текущее время рекламы последним в списке. Если да, то соответствующая задача в базе данных помечается как завершенная, и ей присваивается дата завершения.
                        }

                        await playerContext.SaveChangesAsync();
                        await playerContext.CommitTransactionAsync();
                        //Сохраняются все изменения в базе данных, и текущая транзакция подтверждается.
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    //После обработки каждого времени рекламы задержка в выполнении на 1 секунду даёт небольшую паузу перед переходом к следующему элементу.
                }
            }

            if (_generatingTimeTooLong)
            {
                await AnalyzePlaylistsInternal();
                //Если флаг _generatingTimeTooLong установлен в true, что может произойти, если предыдущая операция генерации заняла слишком много времени, метод AnalyzePlaylistsInternal вызывается рекурсивно для повторного анализа плейлистов. Это может помочь завершить обработку, если предыдущая попытка была прервана или не завершена по каким-либо причинам.
            }
            //Метод AnalyzePlaylistsInternal: Содержит логику для анализа и генерации плейлистов. Метод извлекает задачи по генерации плейлистов, которые еще не завершены, а также связанные с ними времена рекламы. Для каждого времени рекламы и соответствующего объекта (например, песни или видео) генерируется новый плейлист, если старый уже существует, он удаляется. После генерации нового плейлиста и обновления базы данных соответствующая задача помечается как завершенная.
        }

        private class PlaylistBasicInfo
        {
            public PlayerTask PlayerTask { get; set; }
            public List<AdTime> AdTimes { get; set; }
            //Класс PlaylistBasicInfo: Используется для группировки информации о задаче генерации плейлиста и связанных с ней временах рекламы, что облегчает передачу данных в процессе генерации.
        }
    }
}

//Код реализует типичный шаблон для фоновых служб в ASP.NET Core, позволяющий регулярно выполнять сложные задачи по обработке данных и обновлению базы данных, в данном случае для управления и генерации плейлистов с рекламой. Это включает в себя работу с Entity Framework Core для доступа и обновления данных, а также применение асинхронного программирования для улучшения производительности и отзывчивости службы.