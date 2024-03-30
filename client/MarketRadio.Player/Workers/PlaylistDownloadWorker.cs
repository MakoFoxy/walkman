using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.Models;
using MarketRadio.Player.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Workers
{//Класс PlaylistDownloadWorker является фоновой службой в системе медиаплеера, задачей которой является регулярное обновление и загрузка плейлистов и треков на определенное количество дней вперед, указанное в константе MaxDownloadPlaylistDays.
    public class PlaylistDownloadWorker : PlayerBackgroundServiceBase
    {
        private const int MaxDownloadPlaylistDays = 1;

        private readonly IServiceProvider _provider;
        private readonly ILogger<PlaylistDownloadWorker> _logger;
        private readonly PlayerStateManager _stateManager;
        private readonly IWebHostEnvironment _env;

        public PlaylistDownloadWorker(IServiceProvider provider,
            ILogger<PlaylistDownloadWorker> logger,
            PlayerStateManager stateManager,
            IWebHostEnvironment env) : base(stateManager)
        {
            _provider = provider;
            _logger = logger;
            _stateManager = stateManager;
            _env = env;
            //             IServiceProvider _provider: используется для доступа к сервисам приложения.
            // ILogger<PlaylistDownloadWorker> _logger: предоставляет функционал логирования.
            // PlayerStateManager _stateManager: управляет состоянием медиаплеера, включая информацию о текущем и загружаемом плейлистах.
            // IWebHostEnvironment _env: позволяет определять, в какой среде (разработка, тестирование, продакшн) запущено приложение.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {//Основной метод службы, который выполняется в цикле, пока не будет запрошена отмена через stoppingToken. Метод выполняет следующие действия:
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.WhenAll(WaitForObject(stoppingToken), WaitForPlaylist(stoppingToken)); //Ожидает наличия объекта и плейлиста.

                try
                {
                    if (!_stateManager.PlaylistIsDownloading)
                    {
                        // TODO перенести в конфигурацию
                        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); //Проверяет, идет ли в данный момент загрузка плейлиста. Если нет, ожидает 5 минут и снова проверяет. Это предотвращает одновременную загрузку нескольких плейлистов.

                        if (!_stateManager.PlaylistIsDownloading)
                        {
                            await DoWork(stoppingToken); //Если загрузка плейлиста не идет, выполняет метод DoWork для загрузки плейлистов.
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");//В случае исключения логирует ошибку.
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); //Ожидает час до следующей итерации цикла.
            }
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {//Метод DoWork отвечает за загрузку плейлистов и треков:
            var from = DateTime.Today.AddDays(1);
            var to = from.AddDays(MaxDownloadPlaylistDays); //Определяет временной интервал для загрузки плейлистов: начиная с завтрашнего дня на количество дней, указанное в MaxDownloadPlaylistDays.

            using var scope = _provider.CreateScope(); 
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>(); 
            var playlistService = scope.ServiceProvider.GetRequiredService<PlaylistService>(); 
            var trackService = scope.ServiceProvider.GetRequiredService<TrackService>(); //Для каждого трека в этих плейлистах проверяет, загружен ли он уже, и если нет — загружает трек.

            var playlist = await context.Playlists.OrderByDescending(p => p.Date).FirstOrDefaultAsync(stoppingToken); 

            if (playlist == null)
            {
                return;
            }

            var playlists = new List<PlaylistDto>(); //Если плейлист доступен, загружает плейлисты за указанный интервал дней.

            for (var i = from; i <= to; i = i.AddDays(1))
            {
                playlists.Add(await playlistService.LoadPlaylist(i));
            }

            foreach (var track in playlists.SelectMany(p => p.Tracks).DistinctBy(t => t.Id)) //Получает последний доступный плейлист из базы данных.
            {
                await trackService.LoadTrackIfNeeded(track.Id);

                if (_env.IsProduction())
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); //В продакшн-среде между загрузками треков делает паузу в 2 секунды, чтобы снизить нагрузку на сервер или внешние сервисы.
                }
            }
        }
        //PlaylistDownloadWorker играет важную роль в поддержании актуальности контента медиаплеера, автоматизируя процесс загрузки плейлистов и треков. Это помогает обеспечить непрерывное воспроизведение и улучшает пользовательский опыт, минимизируя риски простоев или повторений контента. Внедрение такой службы требует тщательного планирования и тестирования, особенно в отношении управления ресурсами и сетевых запросов.
    }
}