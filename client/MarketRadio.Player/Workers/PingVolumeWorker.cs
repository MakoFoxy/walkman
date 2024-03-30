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
{ //Класс PingVolumeWorker является примером фоновой службы в .NET, предназначенной для периодической отправки данных о текущей громкости в определенный сервис или компонент (Bus в данном случае). Это может быть полезно, например, для синхронизации настроек громкости между различными компонентами системы. Давайте разберем ключевые моменты реализации этой службы:
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
            //             Конструктор PingVolumeWorker принимает несколько зависимостей:

            //     PlayerStateManager для доступа к состоянию плеера и управлением его поведением.
            //     ILogger<PingVolumeWorker> для логирования.
            //     IServiceProvider для доступа к сервисам приложения.
            //     Bus для отправки сообщений о текущей громкости.

            // Эти зависимости инжектируются при создании экземпляра службы и используются в его методах.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.WhenAll(WaitForObject(stoppingToken), WaitForPlaylist(stoppingToken)); //Сначала ожидается выполнение двух задач: WaitForObject и WaitForPlaylist, что может представлять собой ожидание готовности каких-то данных или состояний.

            while (!stoppingToken.IsCancellationRequested) //Пока не будет запрошена отмена через stoppingToken, служба периодически (каждые 0.25 секунды) выполняет метод PingVolume.В случае возникновения исключения оно логируется.
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
        { //Метод PingVolume отвечает за логику получения и отправки текущей громкости:
            var objectInfo = await GetObject(stoppingToken);
            var volume = GetVolumeOnCurrentHour(objectInfo);
            await _bus.PingCurrentVolume(volume);
        }

        private async Task<ObjectInfo> GetObject(CancellationToken stoppingToken)
        { //Метод GetObject предназначен для получения данных об объекте из контекста базы данных с использованием асинхронного доступа и поддержкой отмены операции.
            using var scope = _serviceProvider.CreateScope(); //Используя IServiceProvider, создается новый скоуп сервисов, из которого извлекается PlayerContext для доступа к данным.
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>(); 
            var objectInfo = await context.ObjectInfos.AsNoTracking().SingleAsync(stoppingToken); //Получается информация о текущем объекте (возможно, треке или рекламе) без отслеживания изменений (AsNoTracking).

            return objectInfo;
        }

        private int GetVolumeOnCurrentHour(ObjectInfo objectInfo)
        {//Этот метод вычисляет значение громкости для текущего часа на основе информации об объекте и его настроек. Логика зависит от типа объекта (музыка или реклама) и использует настройки громкости, специфичные для каждого часа дня.
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
        //Этот код демонстрирует использование фоновых служб для периодической асинхронной работы в .NET Core. Важными аспектами являются использование CancellationToken для управления отменой задач, внедрение зависимостей для доступа к сервисам и ресурсам приложения, и логирование для отслеживания работы службы. PingVolumeWorker может быть частью большей системы, где синхронизация настроек громкости имеет важное значение.
    }
}