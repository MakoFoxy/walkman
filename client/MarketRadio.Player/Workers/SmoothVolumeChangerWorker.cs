using System;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Helpers;
using MarketRadio.Player.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Workers
{
    public class SmoothVolumeChangerWorker : PlayerBackgroundServiceBase
    { //Этот код представляет собой фоновую службу SmoothVolumeChangerWorker, которая реализует постепенное снижение громкости воспроизведения перед началом следующего трека в медиаплеере. Давайте разберем ключевые аспекты реализации этой службы.
        private const int SecondBeforeNextTrack = 3; // За 3 секунды до окончания трека будет снижать громкость  
        private const int VolumeDownFor = 30;  // На 30% снижаем громкость относительно текущей громкости
        private const int VolumeSubtractInTick = 5;  // На 5 процентов снижаем громкость каждый тик

        private readonly PlayerStateManager _stateManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly Bus _bus;
        private readonly ILogger<SmoothVolumeChangerWorker> _logger;

        public SmoothVolumeChangerWorker(
            PlayerStateManager stateManager,
            IServiceProvider serviceProvider,
            Bus bus,
            ILogger<SmoothVolumeChangerWorker> logger) : base(stateManager)
        {
            _stateManager = stateManager;
            _serviceProvider = serviceProvider;
            _bus = bus;
            _logger = logger;
            //    Поля, такие как _stateManager, _serviceProvider, _bus, и _logger, используются для управления состоянием плеера, доступа к сервисам, коммуникации с другими частями системы и логирования соответственно.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var waitForObject = WaitForObject(stoppingToken);
            var waitForPlaylist = WaitForPlaylist(stoppingToken);
            //Подготовка к Работе: Сначала служба ожидает готовности объекта (возможно, текущего воспроизводимого элемента) и плейлиста к работе через вызовы WaitForObject и WaitForPlaylist.
            await Task.WhenAll(waitForObject, waitForPlaylist);

            while (!stoppingToken.IsCancellationRequested)
            {//Основной Цикл: В бесконечном цикле, который продолжается до запроса на отмену, выполняется следующая логика:
                var nextTrack = _stateManager.NextTrack;

                if (nextTrack?.PlayingDateTime.RoundToSeconds() == DateTime.Now.AddSeconds(SecondBeforeNextTrack).RoundToSeconds())
                {//Проверка времени начала следующего трека. Если до его начала остается 3 секунды (SecondBeforeNextTrack), начинается процесс снижения громкости.
                    _logger.LogInformation("Volume level changing for track {@Track}", _stateManager.CurrentTrack);
                    ObjectInfo objectInfo; //Получение информации о текущем объекте и настройках громкости для рекламы и музыки в зависимости от времени суток.
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
                        objectInfo = await context.ObjectInfos.AsNoTracking().SingleAsync(stoppingToken);
                    }

                    var hoursFromBeginTime = (DateTime.Now.TimeOfDay - objectInfo.BeginTime).Hours;

                    var volume = nextTrack.Type == Track.Advert
                        ? objectInfo.Settings!.AdvertVolumeComputed[hoursFromBeginTime]
                        : objectInfo.Settings!.MusicVolumeComputed[hoursFromBeginTime];
                    _logger.LogInformation("Volume level selected {VolumeLevel}", volume);

                    var neededVolume = volume - VolumeDownFor; //Расчет необходимого уровня громкости после снижения (neededVolume) и последовательное уменьшение текущего уровня громкости на VolumeSubtractInTick процентов каждый "тик", пока не будет достигнут расчетный уровень.
                    _logger.LogInformation("Needed Volume level {VolumeLevel}", neededVolume);

                    while (volume - VolumeDownFor > neededVolume)
                    {
                        volume -= VolumeSubtractInTick;
                        await _bus.CurrentVolumeChanged(volume);
                        _logger.LogInformation("Current volume changed for {VolumeLevel}", volume);
                        await Task.Delay(200, stoppingToken);
                    }
                    //                     Служба действует проактивно, анализируя расписание воспроизведения и автоматически адаптируя уровень громкости перед сменой треков, особенно перед рекламными блоками или сменой музыкальных треков.
                    // Реализация предусматривает плавное снижение громкости, чтобы не было резких переходов между треками, что повышает комфортность прослушивания для пользователя.
                    // Использование DI (Dependency Injection) и сервисов для доступа к настройкам и состоянию плеера подчеркивает модульную структуру и возможность гибкой настройки поведения плеера.
                }

                await Task.Delay(TimeSpan.FromSeconds(0.5), stoppingToken); //Задержка: Между итерациями основного цикла делается задержка в 0.5 секунды, чтобы управлять частотой проверок и изменений громкости.
            }
            //Этот код является примером сложного взаимодействия между различными компонентами системы для достижения более высокого уровня пользовательского опыта при воспроизведении мультимедийного контента.
        }
    }
}