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
    {
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
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var waitForObject = WaitForObject(stoppingToken);
            var waitForPlaylist = WaitForPlaylist(stoppingToken);
            await Task.WhenAll(waitForObject, waitForPlaylist);

            while (!stoppingToken.IsCancellationRequested)
            {
                var nextTrack = _stateManager.NextTrack;

                if (nextTrack?.PlayingDateTime.RoundToSeconds() == DateTime.Now.AddSeconds(SecondBeforeNextTrack).RoundToSeconds())
                {
                    _logger.LogInformation("Volume level changing for track {@Track}", _stateManager.CurrentTrack);
                    ObjectInfo objectInfo;
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
                    
                    var neededVolume = volume - VolumeDownFor;
                    _logger.LogInformation("Needed Volume level {VolumeLevel}", neededVolume);

                    while (volume - VolumeDownFor > neededVolume)
                    {
                        volume -= VolumeSubtractInTick;
                        await _bus.CurrentVolumeChanged(volume);
                        _logger.LogInformation("Current volume changed for {VolumeLevel}", volume);
                        await Task.Delay(200, stoppingToken);
                    }
                }
                
                await Task.Delay(TimeSpan.FromSeconds(0.5), stoppingToken);
            }
        }
    }
}