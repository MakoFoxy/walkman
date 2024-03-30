using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Helpers;
using MarketRadio.Player.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Player.ClientIntegration;

namespace MarketRadio.Player.Workers
{
    public class PlaylistWatcherWorker : PlayerBackgroundServiceBase
    {//Этот код представляет собой фоновую службу PlaylistWatcherWorker, которая выполняет несколько ключевых функций по мониторингу и управлению плейлистом в медиаплеере. Вот разбор основных частей кода:
        private readonly PlayerStateManager _stateManager;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Bus _bus;
        private readonly Random _random = new();

        private bool _lastTickIsWorkTime;

        public PlaylistWatcherWorker(PlayerStateManager stateManager,
            ILogger<PlaylistWatcherWorker> logger,
            IServiceProvider serviceProvider,
            Bus bus) : base(stateManager)
        {
            _stateManager = stateManager;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _bus = bus;
            //    Конструктор PlaylistWatcherWorker принимает зависимости, которые инжектируются при создании экземпляра класса: PlayerStateManager для управления состоянием плеера, ILogger для логирования, IServiceProvider для доступа к сервисам и Bus для взаимодействия с другими частями системы. Конструктор передает stateManager в базовый класс PlayerBackgroundServiceBase.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {//    Это основной метод фоновой службы, который выполняется в цикле, пока не будет получен сигнал к остановке через stoppingToken. В методе реализованы вызовы других задач, таких как ожидание доступности плейлиста, чтение списка заблокированных треков и мониторинг текущего трека и плейлиста.
            await WaitForPlaylist(stoppingToken);
            await ReadBanList(stoppingToken); //    Перед началом основного цикла служба ожидает пока не будет доступен плейлист и читает список заблокированных треков из контекста базы данных.

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //TODO рассмотреть этот подход к 24-часовым объектам
                    if (_stateManager.IsOnline)
                    {
                        await WatchOnActualPlaylist();
                    }
                    //    Внутри основного цикла происходит мониторинг текущего состояния плейлиста и треков. Методы проверяют, не истек ли срок актуальности текущего плейлиста, содержится ли текущий трек в списке заблокированных и нужно ли заменить его.
                    await WatchOnBannedTrack(stoppingToken);
                    await WatchOnActualTrack(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                }

                await Task.Delay(TimeSpan.FromSeconds(.5), stoppingToken);
            }
        }

        private async Task ReadBanList(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();//    Создает новый скоуп для DI-контейнера и получает PlayerContext для доступа к базе данных.
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var bannedTracks = await context.BanLists.Select(bl => bl.MusicTrackId).ToListAsync(stoppingToken); //    Получает список идентификаторов заблокированных треков из базы данных асинхронно, используя stoppingToken для возможной отмены операции.
            _stateManager.BannedTracks.AddRange(bannedTracks);
        }

        private async Task WatchOnBannedTrack(CancellationToken stoppingToken)
        {
            if (_stateManager.CurrentTrack == null)
            {
                return;
            }

            if (_stateManager.BannedTracks.Contains(_stateManager.CurrentTrack.Id)) //    Проверяет, содержится ли текущий трек в списке заблокированных.
            {
                _logger.LogInformation("Track {@Track} in ban list, changing to random track", _stateManager.CurrentTrack);
                var musicList = _stateManager.Playlist!.Tracks
                    .Where(t => t.Type == Track.Music && t.Id != _stateManager.CurrentTrack.Id)
                    .ToList();

                var randomMusicTrack = musicList[_random.Next(0, musicList.Count)];//    Если текущий трек заблокирован, выбирает случайный трек из доступных в плейлисте, который не является заблокированным.
                _logger.LogInformation("Track {@Track} is selected to replace track {@BannedTrack}", randomMusicTrack, randomMusicTrack);
                await ChangeCurrentTrack(randomMusicTrack);
            }
        }

        private async Task WatchOnActualPlaylist()
        {//    Проверяет, соответствует ли дата текущего плейлиста текущему дню. Если нет, загружает актуальный плейлист на сегодня.
            if (_stateManager.Playlist == null)
            {
                return;
            }

            if (_stateManager.Playlist.Date.Date != DateTime.Today)
            {
                using var scope = _serviceProvider.CreateScope();
                var playlistService = scope.ServiceProvider.GetRequiredService<PlaylistService>();
                var playlist = await playlistService.LoadPlaylist(DateTime.Today);
                await _stateManager.ChangePlaylist(playlist);
            }
        }

        private async Task WatchOnActualTrack(CancellationToken stoppingToken)
        {//Метод WatchOnActualTrack в классе PlaylistWatcherWorker отвечает за мониторинг и управление текущим треком на основе заданных условий. Вот подробный разбор ключевых аспектов этого метода:
            if (_stateManager.Object == null)
            {
                return;//Если текущий объект (возможно, воспроизводимый элемент) не определен, метод ничего не делает и возвращается.
            }

            if (_stateManager.Object.FreeDays.Contains(DateTime.Now.DayOfWeek))
            {
                return;//Если текущий день недели является одним из "свободных" дней для объекта, в которые не требуется воспроизведение, метод также завершается.
            }

            if (_lastTickIsWorkTime && !NowTheWorkingTime)
            {
                _logger.LogInformation("Stop playing");
                await _bus.StopPlaying();
                _lastTickIsWorkTime = false; //Если предыдущая проверка (_lastTickIsWorkTime) указывала на рабочее время, но текущая проверка (NowTheWorkingTime) показывает, что рабочее время закончилось, служба отправляет команду остановить воспроизведение.
            }

            if (!NowTheWorkingTime)
            {
                return; //Если текущее время не является рабочим, метод завершается.
            }

            _lastTickIsWorkTime = true;

            var playlistTracks = _stateManager.Playlist?.Tracks ?? ArraySegment<TrackDto>.Empty;
            var tracks = playlistTracks
                .Where(t => t.PlayingDateTime.TimeOfDay <= DateTime.Now.TimeOfDay &&
                            t.PlayingDateTime.AddSeconds(t.Length).TimeOfDay > DateTime.Now.TimeOfDay)
                .Where(t => !_stateManager.BannedTracks.Contains(t.Id))
                .ToList(); //Из списка треков в плейлисте выбираются те, которые должны играть в текущий момент времени и не входят в список заблокированных.

            TrackDto? track = null;

            if (!tracks.Any())
            {
                return;
            }

            if (tracks.Count == 1)
            {
                track = tracks[0]; //Если доступен только один трек, выбирается он.
            }

            if (tracks.Any(t => t.Type == Track.Advert))
            {
                track = tracks.First(t => t.Type == Track.Advert); //Если среди доступных треков есть реклама (Track.Advert), выбирается первый рекламный трек.
            }

            if (NowTheWorkingTime && track == null)
            {
                _logger.LogWarning("Track is null in work time {CurrentTime}", DateTime.Now); //Если после всех проверок трек так и не был выбран, выводится предупреждение.
            }

            if (track == null)
            {
                return;
            }

            if (_stateManager.CurrentTrack?.UniqueId != track.UniqueId) //Если текущий трек отличается от выбранного, происходит проверка существования файла трека. Если файл существует, трек становится текущим.
            {
                if (!File.Exists(Path.Combine(DefaultLocations.TracksPath, track.UniqueName)))
                {
                    _logger.LogWarning("Track {TrackName} not exists", track.UniqueName);
                }

                await ChangeCurrentTrack(track);

                _logger.LogInformation(
                    "Current track changed {@Track} next track start time {NextTrackStartTime}",
                    track, _stateManager.NextTrack?.PlayingDateTime);
            }
        }

        private async Task ChangeCurrentTrack(TrackDto track)
        {
            await _stateManager.ChangeCurrentTrack(track); //Этот метод делегирует вызов методу ChangeCurrentTrack объекта _stateManager для смены текущего трека.
        }
    }
    //Метод WatchOnActualTrack реализует логику мониторинга и управления воспроизведением в зависимости от времени и состояния плейлиста, автоматически адаптируясь к изменениям и предотвращая воспроизведение заблокированных треков.
}

// В представленном коде фоновой службы PlaylistWatcherWorker используется один активный плейлист в каждый момент времени. Это можно определить по следующим ключевым аспектам кода:

//     Управление одним плейлистом: В коде есть ссылки на _stateManager.Playlist, что указывает на то, что в каждый конкретный момент времени управляется одним плейлистом. Например, метод WatchOnActualPlaylist проверяет, соответствует ли дата текущего плейлиста текущему дню, и если нет, загружает новый актуальный плейлист для сегодня. Это говорит о том, что плейлист существует в единственном экземпляре и обновляется при необходимости.

//     Операции с плейлистом: Методы, такие как WatchOnBannedTrack и WatchOnActualTrack, работают с текущим плейлистом и текущим треком, что подразумевает, что в данный момент активен только один плейлист.

//     Обновление плейлиста: Когда проверяется актуальность плейлиста (например, в WatchOnActualPlaylist), код предполагает замену текущего плейлиста новым, если дата не соответствует текущему дню. Это действие подтверждает, что система рассчитана на работу с одним активным плейлистом за раз.

//     Структура управления состоянием: PlayerStateManager, как предполагается, содержит состояние плеера, включая информацию о текущем плейлисте и треке. Обращение к этому состоянию происходит через единственный экземпляр состояния, что также указывает на работу с одним плейлистом.

// Исходя из этого, весь код ориентирован на управление, мониторинг и адаптацию работы с одним плейлистом в каждый момент времени, хотя и предусматривает механизмы для его обновления и замены в зависимости от текущих условий и требований.



// В фоновой службе PlaylistWatcherWorker реализованы механизмы для загрузки плейлиста и автоматического управления воспроизведением треков из плейлиста. Давайте рассмотрим, как эти части работают вместе, обеспечивая бесперебойную работу плеера.
// Загрузка Плейлиста

// Методы WaitForPlaylist и WatchOnActualPlaylist отвечают за загрузку и проверку актуальности плейлиста:

//     WaitForPlaylist предположительно ожидает, пока плейлист станет доступен. Хотя сам метод не представлен в предоставленном коде, исходя из названия, можно предположить, что он обеспечивает начальное ожидание доступности плейлиста перед началом основного цикла мониторинга и управления. Этот метод может включать в себя проверку наличия плейлиста на текущий день или ожидание его появления в системе.

//     WatchOnActualPlaylist активно проверяет, соответствует ли дата текущего плейлиста сегодняшнему дню, и, если нет, производит загрузку нового актуального плейлиста для текущей даты. Этот метод гарантирует, что плейлист всегда актуален и соответствует текущему дню, что критически важно для медиаплееров, работающих 24/7, например, в радиостанциях или системах фоновой музыки в общественных местах.

// Автоуправление Воспроизведением

// Метод WatchOnActualTrack отвечает за управление воспроизведением треков на основе текущего времени и состояния плейлиста:

//     Внутри этого метода происходит проверка, соответствует ли текущее время воспроизведения какому-либо треку в плейлисте. Это включает в себя проверку рабочего времени (если применимо), а также убеждение в том, что текущий трек не находится в списке заблокированных.

//     Если текущий трек необходимо заменить (например, он заблокирован или уже не актуален), WatchOnActualTrack осуществляет выбор и переключение на новый трек из плейлиста. Это может включать выбор рекламного трека (если пришло время его воспроизведения) или другого подходящего музыкального трека.

//     Метод также учитывает специфические требования, такие как "свободные дни" или часы, когда воспроизведение треков не требуется, и прекращает воспроизведение в эти периоды.

// Эти механизмы обеспечивают, что плеер автоматически адаптируется к изменениям в плейлисте и контексте воспроизведения, обновляя воспроизводимые треки в соответствии с текущими требованиями и ограничениями. Они существенно повышают удобство управления плейлистом и гарантируют соответствие воспроизводимого контента текущему контексту и предпочтениям слушателей.
