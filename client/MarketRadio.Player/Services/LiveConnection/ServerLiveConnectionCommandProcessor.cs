using System;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Models;
using MarketRadio.Player.Services.System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Player.ClientIntegration;
using Player.ClientIntegration.MusicTrack;
using Player.ClientIntegration.Object;
using Player.ClientIntegration.System;

namespace MarketRadio.Player.Services.LiveConnection
{
    public class ServerLiveConnectionCommandProcessor
    //ServerLiveConnectionCommandProcessor — это класс, который служит обработчиком команд от сервера через живое соединение, устанавливаемое с помощью SignalR. Этот класс подписывается на различные события (или команды), которые сервер может отправлять клиенту, и реализует логику обработки этих событий. Вот основные компоненты и логика его работы:
    {
        private readonly ILogger<ServerLiveConnectionCommandProcessor> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly PlayerStateManager _stateManager;
        private readonly LogsUploader _logsUploader;
        private HubConnection _connection = null!;

        public ServerLiveConnectionCommandProcessor(
            ILogger<ServerLiveConnectionCommandProcessor> logger,
            IServiceProvider serviceProvider,
            PlayerStateManager stateManager,
            LogsUploader logsUploader)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _stateManager = stateManager;
            _logsUploader = logsUploader;
            //ILogger<ServerLiveConnectionCommandProcessor> для логирования действий обработчика команд.
            // IServiceProvider для доступа к другим сервисам приложения внутри методов обработки событий.
            // PlayerStateManager для управления состоянием плеера, например, для обновления информации об объекте воспроизведения или изменения громкости.
            // LogsUploader для выполнения задачи по загрузке логов на сервер, если такая команда поступает от сервера.
        }

        public void Subscribe(HubConnection connection)
        //Метод Subscribe используется для подписки на события (или команды) сервера. Он принимает HubConnection — соединение с сервером через SignalR, и настраивает обработчики для различных типов событий:
        {
            _connection = connection;
            _connection.On<BanMusicTrackDto>(OnlineEvents.MusicBanned, MusicBanAccepted);
            _connection.On<ObjectVolumeChanged>(OnlineEvents.ObjectVolumeChanged, ObjectVolumeChangeAccepted);
            _connection.On<DownloadLogsRequest>(OnlineEvents.DownloadLogs, DownloadLogsAccepted);
            _connection.On(OnlineEvents.CurrentTrackRequest, CurrentTrackRequest);
            _connection.On<ObjectInfo>(OnlineEvents.ObjectInfoReceived, ObjectInfoAccepted);
            //    MusicBanned для обработки запрета на музыкальный трек.
            // ObjectVolumeChanged для изменения громкости объекта.
            // DownloadLogs для запроса на загрузку логов.
            // CurrentTrackRequest для запроса информации о текущем треке.
            // ObjectInfoReceived для получения и обновления информации об объекте.
        }

        private async Task ObjectInfoAccepted(ObjectInfo info)
        {//Метод ObjectInfoAccepted демонстрирует обработку события получения информации об объекте (ObjectInfoReceived). При получении этого события:
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var objectInfo = await context.ObjectInfos.SingleAsync();

            objectInfo.Bin = info.Bin;
            objectInfo.City = info.City;
            objectInfo.Name = info.Name;
            objectInfo.Settings = info.Settings ?? objectInfo.Settings;
            objectInfo.BeginTime = info.BeginTime;
            objectInfo.EndTime = info.EndTime;
            objectInfo.FreeDays = info.FreeDays;

            await context.SaveChangesAsync();

            await _stateManager.UpdateObject(new ObjectInfoDto
            {
                Id = objectInfo.Id,
                Bin = objectInfo.Bin,
                Name = objectInfo.Name,
                BeginTime = objectInfo.BeginTime,
                EndTime = objectInfo.EndTime,
                FreeDays = objectInfo.FreeDays,
                Settings = new SettingsDto
                {
                    MusicVolume = objectInfo.Settings!.MusicVolumeComputed,
                    SilentTime = objectInfo.Settings.SilentTime,
                    AdvertVolume = objectInfo.Settings.AdvertVolumeComputed,
                    IsOnTop = objectInfo.Settings.IsOnTop,
                }
            });
            //    Создается новый сервисный облачный контекст (IServiceScope) для доступа к PlayerContext — контексту базы данных приложения.
            // Извлекается существующая информация об объекте и обновляется на основе полученных данных.
            // Сохраняются изменения в базе данных.
            // Обновляется состояние объекта в PlayerStateManager, чтобы отразить полученные изменения.
        }
        //Этот процесс показывает, как приложение реагирует на команды сервера в реальном времени, обновляя своё состояние и данные в соответствии с этими командами. Это обеспечивает динамичное и синхронизированное поведение между клиентом и сервером, позволяя серверу напрямую влиять на работу клиентского приложения.
        private Task CurrentTrackRequest()
        { //Этот метод вызывается при получении запроса от сервера на предоставление информации о текущем треке, который играет в плеере. В ответ на запрос:
            var onlineObjectInfo = new OnlineObjectInfo
            {//Создаётся объект OnlineObjectInfo, который заполняется текущей датой и временем, идентификатором объекта (воспроизводящего устройства) и, если трек воспроизводится, уникальным идентификатором трека и количеством секунд с момента его начала воспроизведения.
                Date = DateTime.Now,
                ObjectId = _stateManager.Object!.Id,
            };
            //Информация отправляется обратно на сервер через живое соединение с использованием метода SendAsync и имени события OnlineEvents.CurrentTrackResponse.
            if (_stateManager.CurrentTrack != null)
            {
                onlineObjectInfo.CurrentTrack = _stateManager.CurrentTrack.UniqueId;
                onlineObjectInfo.SecondsFromStart = (int)onlineObjectInfo.Date.Subtract(_stateManager.CurrentTrack.PlayingDateTime).TotalSeconds;
            }

            // return _connection.SendAsync(OnlineEvents.CurrentTrackResponse, JsonConvert.SerializeObject(onlineObjectInfo));
            return _connection.SendAsync(OnlineEvents.CurrentTrackResponse, onlineObjectInfo);
        }

        private async Task ObjectVolumeChangeAccepted(ObjectVolumeChanged objectVolumeChanged)
        {//Вызывается при получении команды от сервера на изменение громкости объекта (воспроизводящего устройства) для конкретного часа дня. В ответ на команду:
            using var scope = _serviceProvider.CreateScope();
            //Извлекаются настройки громкости из контекста базы данных и обновляются согласно полученным данным.
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            var objectInfo = await context.ObjectInfos.SingleAsync();
            objectInfo.Settings!.AdvertVolumeComputed[objectVolumeChanged.Hour] = objectVolumeChanged.AdvertVolume;
            objectInfo.Settings.MusicVolumeComputed[objectVolumeChanged.Hour] = objectVolumeChanged.MusicVolume;
            await context.SaveChangesAsync();
            //Сохраняются изменения в базе данных.
            _stateManager.Object!.Settings.AdvertVolume = objectInfo.Settings.AdvertVolumeComputed;
            _stateManager.Object!.Settings.MusicVolume = objectInfo.Settings.MusicVolumeComputed;
            await _stateManager.ChangeObjectVolume(objectVolumeChanged);
            //Обновляется локальное состояние громкости в PlayerStateManager и выполняется соответствующее действие по изменению громкости.
        }

        private async Task MusicBanAccepted(BanMusicTrackDto ban)
        {//Обрабатывает команду от сервера на блокировку (бан) музыкального трека. В ответ на такую команду:
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
            context.BanLists.Add(new BanList
            {
                ObjectId = ban.ObjectId,
                MusicTrackId = ban.MusicId,
            });
            await context.SaveChangesAsync();
            _stateManager.BannedTracks.Add(ban.MusicId);
            //    Добавляет информацию о блокировке трека в базу данных и обновляет список заблокированных треков в PlayerStateManager.
        }

        private async Task DownloadLogsAccepted(DownloadLogsRequest downloadLogsModel)
        {//Вызывается при получении команды на загрузку логов на сервер. Этот метод:
            _logger.LogInformation("DownloadLogs accepted");
            try
            {//Логирует начало операции загрузки логов.
                await _logsUploader.UploadLogs(downloadLogsModel);//Вызывает метод UploadLogs у LogsUploader для выполнения операции загрузки.В случае успеха или неудачи логирует соответствующее сообщение.
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                return;
            }

            _logger.LogInformation("DownloadLogs have been sent to the server");
        }
        //Эти методы демонстрируют, как приложение может динамично реагировать на команды сервера, изменяя своё состояние, выполняя действия по запросу сервера и предоставляя обратную связь. Это обеспечивает гибкое взаимодействие между клиентом и сервером, позволяя серверу управлять поведением клиента в реальном времени.
    }
}