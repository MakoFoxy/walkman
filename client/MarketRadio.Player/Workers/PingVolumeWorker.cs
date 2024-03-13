using System;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Workers
{
    public class PingVolumeWorker : PlayerBackgroundServiceBase
    {
        private readonly PlayerStateManager _stateManager;
        private readonly ILogger<PingVolumeWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Bus _bus;
        
        public PingVolumeWorker(
            PlayerStateManager stateManager,
            ILogger<PingVolumeWorker> logger,
            IServiceProvider serviceProvider,
            Bus bus) : base(stateManager)
        {
            _stateManager = stateManager;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _bus = bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.WhenAll(WaitForObject(stoppingToken), WaitForPlaylist(stoppingToken));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PingVolume(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                }
                await Task.Delay(TimeSpan.FromSeconds(0.25), stoppingToken);
            }
        }

        private async Task PingVolume(CancellationToken stoppingToken)
        {
            var objectInfo = await GetObject(stoppingToken);
            var volume = GetVolumeOnCurrentHour(objectInfo);
            await _bus.PingCurrentVolume(volume);
        }

        private async Task<ObjectInfo> GetObject(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var objectInfo = await context.ObjectInfos.AsNoTracking().SingleAsync(stoppingToken);

            return objectInfo;
        }

        private int GetVolumeOnCurrentHour(ObjectInfo objectInfo)
        {
            var currentHours = DateTime.Now.TimeOfDay.Hours;

            var nextTrack = _stateManager.NextTrack;

            if (nextTrack == null)
            {
                return 0;
            }
            
            return nextTrack.Type == Track.Advert
                ? objectInfo.Settings!.AdvertVolumeComputed[currentHours]
                : objectInfo.Settings!.MusicVolumeComputed[currentHours];
        }
    }
}